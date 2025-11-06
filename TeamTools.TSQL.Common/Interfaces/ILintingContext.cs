using System.IO;

namespace TeamTools.Common.Linting
{
    public interface ILintingContext
    {
        string FilePath { get; }

        TextReader FileContents { get; }

        void ReportViolation(RuleViolation violation);
    }
}
