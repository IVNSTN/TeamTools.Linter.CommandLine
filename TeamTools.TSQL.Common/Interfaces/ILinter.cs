using System.Collections.Generic;

namespace TeamTools.Common.Linting
{
    public interface ILinter
    {
        void PerformAction(ILintingContext context);

        void Init(string configPath, IReporter reporter, string cultureCode);

        void SetReporter(IReporter reporter);

        void LoadConfig(string configPath);

        int GetRulesCount();

        IEnumerable<string> GetSupportedFiles();
    }
}
