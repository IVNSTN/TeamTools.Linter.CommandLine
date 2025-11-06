using System.IO;

namespace TeamTools.Common.Linting
{
    public class LintingContext : ILintingContext
    {
        private readonly IReporter reporter;

        public LintingContext(string filePath, TextReader src, IReporter repoter)
        {
            this.FilePath = filePath;
            this.FileContents = src;
            this.reporter = repoter;
        }

        public string FilePath { get; }

        public TextReader FileContents { get; }

        public void ReportViolation(RuleViolation violation)
        {
            // violation.FileName = this.FilePath;
            reporter.ReportViolation(violation);
        }
    }
}
