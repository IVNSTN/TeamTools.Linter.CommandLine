using System.IO;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    internal class FileReporter : ConsoleReporter
    {
        private readonly TextWriter output;
        private readonly IReportOutputFormatter formatter;

        public FileReporter(TextWriter output, IReportOutputFormatter formatter)
        {
            this.output = output;
            this.formatter = formatter;
            ShouldCollectViolations = true;
        }

        public override void Report(string msg)
        {
            // TODO : unexpected behavior change: switching from regular output to stderr
            // only because "output file" requested
            ReportFailure(msg);
        }

        public override void ReportResults()
        {
            try
            {
                formatter.Write(this.Violations, this.output);
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
