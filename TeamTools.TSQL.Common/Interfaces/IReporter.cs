namespace TeamTools.Common.Linting
{
    public interface IReporter
    {
        void ReportViolation(RuleViolation violation);

        void Report(string msg);

        void ReportFailure(string error);
    }
}
