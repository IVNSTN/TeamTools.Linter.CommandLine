using System.IO;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    internal class FileReporter : IReporterDecorator
    {
        private readonly TextWriter output;
        private readonly IReportOutputFormatter formatter;
        private readonly ConsoleReporter messageReporter;

        // TODO : Embeding ConsoleReporter is not required for writing output into dedicated file
        // consider refactoring and getting rid of this dependency.
        public FileReporter(TextWriter output, ConsoleReporter messageReporter, IReportOutputFormatter formatter)
        {
            this.output = output;
            this.formatter = formatter;
            this.messageReporter = messageReporter;

            // TODO : too much magic
            messageReporter.ShouldCollectViolations = true;
            messageReporter.ShouldReporViolationsAsFailures = true;
        }

        public int ViolationCount => messageReporter.ViolationCount;

        public void ReportFailure(string error) => messageReporter.ReportFailure(error);

        public void ReportViolation(RuleViolation violation) => messageReporter.ReportViolation(violation);

        // TODO : unexpected behavior change: switching from regular output to stderr
        // only because "output file" requested
        public virtual void Report(string msg) => ReportFailure(msg);

        public void ReportResults()
        {
            try
            {
                formatter.Write(messageReporter.Violations, this.output);
            }
            finally
            {
                Dump();
            }
        }

        protected void Dump()
        {
            output.Flush();
            output.Close();
        }
    }
}
