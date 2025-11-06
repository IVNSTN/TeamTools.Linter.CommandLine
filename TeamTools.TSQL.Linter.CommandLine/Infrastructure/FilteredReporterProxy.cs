using System;
using System.Diagnostics;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class FilteredReporterProxy : IReporterDecorator
    {
        private readonly IReporterDecorator proxiedReporter;
        private readonly Severity minimalSeverity;

        public FilteredReporterProxy(IReporterDecorator proxiedReporter, Severity minimalSeverity)
        {
            this.proxiedReporter = proxiedReporter ?? throw new ArgumentNullException(nameof(proxiedReporter));
            this.minimalSeverity = minimalSeverity;
        }

        public int ViolationCount => proxiedReporter.ViolationCount;

        public void Report(string msg) => proxiedReporter.Report(msg);

        public void ReportFailure(string error) => proxiedReporter.Report(error);

        public void ReportResults() => proxiedReporter.ReportResults();

        public void ReportViolation(RuleViolation violation)
        {
            if (violation.ViolationSeverity >= minimalSeverity)
            {
                proxiedReporter.ReportViolation(violation);
            }
            else
            {
                Debug.WriteLine($"Rule ${violation.RuleId} violation skipped due to severity level '${violation.SeverityName}'");
            }
        }
    }
}
