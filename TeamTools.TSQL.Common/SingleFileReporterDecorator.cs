namespace TeamTools.Common.Linting
{
    public class SingleFileReporterDecorator : IReporter
    {
        private static readonly string Delim = ": ";
        private readonly string filePath;
        private readonly IReporter reporter;

        public SingleFileReporterDecorator(string filePath, IReporter reporter)
        {
            this.filePath = filePath;
            this.reporter = reporter;
        }

        public void Report(string msg)
        {
            reporter.Report(string.Concat(filePath, Delim, msg));
        }

        public void ReportFailure(string error)
        {
            reporter.ReportFailure(string.Concat(filePath, Delim, error));
        }

        public void ReportViolation(RuleViolation violation)
        {
            violation.FileName = this.filePath;
            reporter.ReportViolation(violation);
        }
    }
}
