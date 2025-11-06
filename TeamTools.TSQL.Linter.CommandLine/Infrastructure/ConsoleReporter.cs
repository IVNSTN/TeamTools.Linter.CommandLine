// original: https://github.com/tsqllint/tsqllint/blob/main/source/TSQLLint.Infrastructure/Reporters/ConsoleReporter.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class ConsoleReporter : IReporter, IReporterDecorator
    {
        protected const string ErrorSeverity = "error";
        private readonly ConcurrentBag<RuleViolation> ruleViolations = new ConcurrentBag<RuleViolation>();
        private readonly string basePath;
        private int errorCount;

        public ConsoleReporter(string basePath, Encoding outputEncoding)
        {
            Console.OutputEncoding = outputEncoding;
            this.basePath = basePath;
        }

        public ConsoleReporter(string basePath = "") : this(basePath, Encoding.UTF8)
        { }

        public bool ReporterMuted { get; set; }

        public bool ShouldCollectViolations { get; set; }

        public IEnumerable<RuleViolation> Violations => ruleViolations.ToArray();

        public int ViolationCount => errorCount;

        public void ClearViolations()
        {
            ruleViolations.Clear();
        }

        [ExcludeFromCodeCoverage]
        public virtual void Report(string message)
        {
            NonBlockingConsole.WriteLine(message);
        }

        [ExcludeFromCodeCoverage]
        public virtual void ReportFailure(string error)
        {
            NonBlockingConsole.WriteError(error);
        }

        public virtual void ReportResults()
        {
            Report(string.Format("Errors: {0}", errorCount));
        }

        public void ReportViolation(RuleViolation violation)
        {
            if (ShouldCollectViolations)
            {
                ruleViolations.Add(violation);
            }

            // If `--fix` is turned on sometimes the program runs a couple of times
            // caused by fixing another rule violation. This condition prevents double or tripple counting.
            if (!ReporterMuted)
            {
                errorCount++;
            }

            // TODO : no, this path reduction has to occure elsewhere before getting to reporter
            string msg = violation.ToString();
            if (!string.IsNullOrEmpty(basePath) && msg.StartsWith(basePath, System.StringComparison.OrdinalIgnoreCase))
            {
                msg = msg.Substring(basePath.Length).TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            Report(msg);
        }
    }
}
