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
        private const string DetailsDelimiter = ": ";
        // key - datatype, value - list of rule instances
        private readonly Dictionary<string, List<RuleInstance<T>>> rules = new Dictionary<string, List<RuleInstance<T>>>(StringComparer.OrdinalIgnoreCase);
        // key - file ext, value - list of rule IDs
        private readonly Dictionary<string, Dictionary<string, T>> rulesPerFileExt = new Dictionary<string, Dictionary<string, T>>(StringComparer.OrdinalIgnoreCase);
        private readonly RuleViolationReporterProxy reporter;
        private readonly IRuleFactory<T> rulesFactory;
        private readonly IRuleClassFinder ruleClassFinder;
        private readonly BaseLintingConfig config;
        private readonly IFileParser<TP> parser;
        private readonly Dictionary<string, T> emptySet = new Dictionary<string, T>();
        private int ruleCount = 0;

        // TODO : move parser away - it is not related to "rule collection"
        protected BaseRuleCollectionHandler(IReporter reporter, IRuleFactory<T> rulesFactory, IRuleClassFinder ruleClassFinder, IFileParser<TP> parser, BaseLintingConfig config)
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
            Debug.Assert(reporter.Context.Value is null, "reporter.Context.Value contains garbage");

            reporter.Context.Value = context;

            try
            {
                var taskParse = Task.Run(() => parser.Parse(context));
                var taskRules = Task.Run(() => GetApplicableRules(Path.GetFileName(context.FilePath)));
                Task.WhenAll(taskParse, taskRules);
                DoApplyRulesTo(taskRules.Result, context.FilePath, taskParse.Result, processor);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                ExpandException(reporter, e, context.FilePath);
            }
            finally
            {
                reporter.Context.Value = null;
            }
        }

        public virtual void MakeRules()
        {
            Debug.Assert(config != null, "config null");

            InitRules();
            CacheRulesForFileExt();
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
                error = $"{ruleMsg}{DetailsDelimiter}{dto.ErrorDetails}";
            }
            else if (!string.IsNullOrEmpty(dto.ErrorDetails))
            {
                error = dto.ErrorDetails;
            }
            else
            {
                error = ruleMsg;
            }

            if (config.RuleSeverity.TryGetValue(ruleId, out Severity svr))
            {
                dto.ViolationSeverity = svr;
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

        protected IEnumerable<KeyValuePair<string, T>> GetApplicableRules(string fileName)
        {
            string fileExt = Path.GetExtension(fileName);

            if (!rulesPerFileExt.TryGetValue(fileExt, out var rulesForFileExt))
            {
                return emptySet;
            }

            IEnumerable<KeyValuePair<string, T>> applicableRules = default;

            var disabledRules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            // rules disabled for any mask matching given file name
            foreach (var filePattern in config.Whitelist.Keys)
            {
                if (filePattern.IsMatch(fileName))
                {
                    var wr = config.Whitelist[filePattern];
                    int n = wr.Count;
                    for (int i = 0; i < n; i++)
                    {
                        disabledRules.Add(wr[i]);
                    }
                }
            }

            if (disabledRules.Count > 0)
            {
                // removing whitelisted rules
                applicableRules = rulesForFileExt
                    .Where(rule => !disabledRules.Contains(rule.Key));

#if DEBUG
                if (!applicableRules.Any())
                {
                    Debug.WriteLine($"All rules disabled for file: {fileName}");
                }
#endif
            }
            else
            {
                return rulesForFileExt;
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

        private void InitRules()
        {
            var foundRules = ruleClassFinder.GetAvailableRuleClasses(config.Rules);

            // TODO : utilize .AsParallel() ?
            foreach (var rule in foundRules)
            {
                if (MakeRule(rule.RuleClassType, rule.RuleId, config.Rules[rule.RuleFullName]) is T ruleInstance)
                {
                    if (config.RuleSeverity.TryGetValue(rule.RuleFullName, out var ruleSeverity))
                    {
                        // TODO : bad crutch
                        config.RuleSeverity[rule.RuleId] = ruleSeverity;
                    }

                    foreach (string dataTypeName in rule.SupportedDataTypes)
                    {
                        if (!rules.TryGetValue(dataTypeName, out var ruleTypes))
                        {
                            ruleTypes = new List<RuleInstance<T>>();
                            rules.Add(dataTypeName, ruleTypes);
                        }

                        ruleTypes.Add(new RuleInstance<T> { RuleFullName = rule.RuleFullName, Rule = ruleInstance });
                    }

                    ruleCount++;
                }
                else
                {
                    Debug.Fail("bad rule type");
                }
            }
        }

        private void CacheRulesForFileExt()
        {
            foreach (var datatype in config.SupportedFiles.Keys)
            {
                if (rules.TryGetValue(datatype, out var applicableRules))
                {
                    var bunchOfRules = applicableRules.ToDictionary(vv => vv.RuleFullName, vv => vv.Rule, StringComparer.OrdinalIgnoreCase);

                    var supportedFiles = config.SupportedFiles[datatype];
                    int n = supportedFiles.Count;
                    for (int i = 0; i < n; i++)
                    {
                        var fileExt = supportedFiles[i];

                        if (!rulesPerFileExt.TryGetValue(fileExt, out var alreadyFoundRules))
                        {
                            rulesPerFileExt[fileExt] = bunchOfRules;
                        }
                        else
                        {
                            foreach (var b in bunchOfRules)
                            {
                                alreadyFoundRules[b.Key] = b.Value;
                            }
                        }
                    }
                }
            }
        }

        private void DoApplyRulesTo(IEnumerable<KeyValuePair<string, T>> applicableRules, string filePath, TP fileContents, Action<T, TP> processor)
        {
            foreach (var rule in applicableRules)
            {
                try
                {
                    processor.Invoke(rule.Value, fileContents);
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
                            FileName = filePath,
                        });
                }
            }
        }
    }
}
