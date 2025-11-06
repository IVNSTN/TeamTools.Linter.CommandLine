using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TeamTools.Common.Linting
{
    public abstract class BaseRuleCollectionHandler<T, TP> : IRuleCollectionHandler<T, TP>
    {
        // key - datatype, value - list of rule instances
        private const string DetailsDelimiter = ": ";
        private readonly IDictionary<string, List<RuleInstance<T>>> rules = new Dictionary<string, List<RuleInstance<T>>>(StringComparer.OrdinalIgnoreCase);
        private readonly RuleViolationReporterProxy reporter;
        private readonly IRuleFactory<T> rulesFactory;
        private readonly IRuleClassFinder ruleClassFinder;
        private readonly BaseLintingConfig config;
        private readonly IFileParser<TP> parser;
        private int ruleCount = 0;

        // TODO : move parser away - it is not related to "rule collection"
        public BaseRuleCollectionHandler(IReporter reporter, IRuleFactory<T> rulesFactory, IRuleClassFinder ruleClassFinder, IFileParser<TP> parser, BaseLintingConfig config)
        {
            Debug.Assert(reporter != null, "reporter not passed");
            Debug.Assert(rulesFactory != null, "rulesFactory not passed");
            Debug.Assert(ruleClassFinder != null, "ruleClassFinder not passed");
            Debug.Assert(config != null, "config not passed");
            Debug.Assert(parser != null, "parser not passed");

            this.reporter = new RuleViolationReporterProxy(reporter);
            this.rulesFactory = rulesFactory;
            this.ruleClassFinder = ruleClassFinder;
            this.config = config;
            this.parser = parser;
        }

        public IDictionary<string, List<RuleInstance<T>>> Rules => rules;

        public int RuleCount() => ruleCount;

        public void ApplyRulesTo(ILintingContext context, Action<T, TP> processor)
        {
            try
            {
                var applicableRules = GetAplicableRules(Path.GetFileName(context.FilePath));
                var fileContents = parser.Parse(context);

                Debug.Assert(reporter.Context.Value is null, "reporter.Context.Value contains garbage");

                Task.Run(() =>
                {
                    reporter.Context.Value = context;

                    foreach (var rule in applicableRules)
                    {
                        try
                        {
                            processor(rule.Value, fileContents);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(string.Format("Failed checking rule {1}: {0} {2}", e.Message, rule.Key, e.StackTrace));
                            Debug.WriteLine(e.StackTrace);

                            // TODO : ensure it is thread safe
                            // TODO: do it another way. in fact it's not rule violation
                            reporter.ReportViolation(
                                new RuleViolation
                                {
                                    RuleId = rule.Key,
                                    Text = string.Format("Failed checking rule {1}: {0}", e.Message, rule.Key),
                                    Line = 0,
                                    Column = 0,
                                    FileName = context.FilePath,
                                });
                        }
                    }

                    reporter.Context.Value = null;
                }).Wait();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                ExpandException(reporter, e, context.FilePath);
            }
        }

        public virtual void MakeRules()
        {
            Debug.Assert(config != null, "config null");

            var foundRules = ruleClassFinder.GetAvailableRuleClasses(config.Rules.Keys);

            foreach (var rule in foundRules)
            {
                if (MakeRule(rule.RuleClassType, rule.RuleId, config.Rules[rule.RuleFullName]) is T ruleInstance)
                {
                    if (config.RuleSeverity.ContainsKey(rule.RuleFullName)
                    && !config.RuleSeverity.ContainsKey(rule.RuleId))
                    {
                        // TODO : bad crutch
                        config.RuleSeverity.Add(rule.RuleId, config.RuleSeverity[rule.RuleFullName]);
                    }

                    foreach (string dataTypeName in rule.SupportedDataTypes)
                    {
                        if (!rules.ContainsKey(dataTypeName))
                        {
                            rules.Add(dataTypeName, new List<RuleInstance<T>>());
                        }

                        rules[dataTypeName].Add(new RuleInstance<T> { RuleFullName = rule.RuleFullName, Rule = ruleInstance });
                    }

                    ruleCount++;
                }
                else
                {
                    Debug.Fail("bad rule type");
                }
            }
        }

        protected object MakeRule(Type ruleClass, string ruleId, string violationMessage)
        {
            return rulesFactory.MakeRule(
                ruleClass,
                (sender, dto) => HandleFileValidationError(ruleId, violationMessage, dto));
        }

        protected void HandleFileValidationError(string ruleId, string ruleMsg, RuleViolationEventDto dto)
        {
            string error;

            if (!string.IsNullOrEmpty(ruleMsg) && !string.Equals(ruleId, ruleMsg)
            && !string.IsNullOrEmpty(dto.ErrorDetails))
            {
                error = ruleMsg + DetailsDelimiter + dto.ErrorDetails;
            }
            else if (!string.IsNullOrEmpty(dto.ErrorDetails))
            {
                error = dto.ErrorDetails;
            }
            else
            {
                error = ruleMsg;
            }

            if (config.RuleSeverity.ContainsKey(ruleId))
            {
                dto.ViolationSeverity = config.RuleSeverity[ruleId];
            }

            reporter?.ReportViolation(new RuleViolation
            {
                RuleId = ruleId,
                Line = dto.Line,
                Column = dto.Column,
                Text = error,
                FragmentLength = dto.FragmentLength,
                ViolationSeverity = dto.ViolationSeverity,
            });
        }

        protected IDictionary<string, T> GetAplicableRules(string fileName)
        {
            // rules for types mapped to given file extension
            var applicableRules = rules
                   .Where(v =>
                       config.SupportedFiles.ContainsKey(v.Key)
                       && config.SupportedFiles[v.Key]
                           .Exists(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                   .SelectMany(v => v.Value)
                   .Distinct()
                   .ToDictionary(vv => vv.RuleFullName, vv => vv.Rule, StringComparer.OrdinalIgnoreCase);

            if (applicableRules.Count == 0)
            {
                Debug.WriteLine("No applicable plugins to this file type: " + fileName);
                return applicableRules;
            }

            // rules disabled for any mask matching given file name
            var disabledRules = config.Whitelist
                .Where(filePattern => filePattern.Key.IsMatch(fileName))
                .SelectMany(rulePattern => rulePattern.Value);

            if (disabledRules.Any())
            {
                // removing whitelisted rules
                applicableRules = applicableRules
                    .Where(rule => !disabledRules.Any(ruleFullName => ruleFullName.Equals(rule.Key, StringComparison.OrdinalIgnoreCase)))
                    .ToDictionary(r => r.Key, r => r.Value);
            }

            if (applicableRules.Count == 0)
            {
                Debug.WriteLine("All rules disabled for file: " + fileName);
                return applicableRules;
            }

            return applicableRules;
        }

        private static void ExpandException(IReporter reporter, Exception e, string filename)
        {
            if (e is AggregateException aggr)
            {
                foreach (var ee in aggr.InnerExceptions)
                {
                    ExpandException(reporter, ee, filename);
                }

                return;
            }

            if (e is ParsingException pars)
            {
                reporter.ReportViolation(
                    new RuleViolation
                    {
                        RuleId = "Parsing",
                        Line = pars.Line,
                        Column = pars.Col,
                        Text = pars.Message,
                        FileName = filename,
                    });

                return;
            }

            reporter.ReportViolation(
                new RuleViolation
                {
                    Text = e.Message,
                    FileName = filename,
                });
        }
    }
}
