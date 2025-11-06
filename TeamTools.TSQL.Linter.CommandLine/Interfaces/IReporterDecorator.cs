using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine
{
    public interface IReporterDecorator : IReporter
    {
        public int ViolationCount { get; }

        void ReportResults();
    }
}
