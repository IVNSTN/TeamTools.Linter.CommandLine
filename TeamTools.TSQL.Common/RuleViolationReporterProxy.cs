using System.Diagnostics;
using System.Threading;

namespace TeamTools.Common.Linting
{
    public class RuleViolationReporterProxy : IReporter
    {
        private readonly IReporter reporter;

        public RuleViolationReporterProxy(IReporter reporter)
        {
            Debug.Assert(reporter != null, "reporter not passed");

            this.reporter = reporter;
        }

        // Linting goes in many threads, split is produced on every analyzed file
        // currently violation reporting callback is embedded into rule during creation
        // thus cannot own file context. During execution specific file context cannot be
        // assigned directly because same rule can analyze different files (process different context)
        // in parallel threads. That's why ThreadLocal is used.
        // TODO : try to refactor this into middleware/callback/chain of responsibility
        // created individually for each file analyze call so rule stays persistent and
        // file-independent and crutches like ThreadLocal are no longer needed.
        public ThreadLocal<ILintingContext> Context { get; } = new ThreadLocal<ILintingContext>(() => null);

        public void ReportViolation(RuleViolation dto)
        {
            var violation = dto.Clone();

            if (Context.Value != null)
            {
                Context.Value.ReportViolation(violation);
            }
            else
            {
                violation.FileName = dto.FileName;
                reporter.ReportViolation(violation);
            }
        }

        public void Report(string msg)
        {
            ReportViolation(new RuleViolation
            {
                Text = msg,
                Line = 0,
                Column = 1,
                ViolationSeverity = Severity.Error,
            });
        }

        public void ReportFailure(string error)
        {
            reporter.ReportFailure(error);
        }
    }
}
