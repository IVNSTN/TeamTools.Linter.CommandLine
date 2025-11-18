using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Interfaces;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    [ExcludeFromCodeCoverage]
    internal class VerboseReporter : IVerboseReporter
    {
        private readonly IReporter reporter;
        private readonly bool isVerboseMod = true;

        public VerboseReporter(IReporter reporter, bool isVerboseMod)
        {
            this.reporter = reporter;
            this.isVerboseMod = isVerboseMod;
        }

        public void ReportVerbose(string msg)
        {
            if (!isVerboseMod)
            {
                Debug.WriteLine(msg);
                return;
            }

            reporter.Report(msg);
        }
    }
}
