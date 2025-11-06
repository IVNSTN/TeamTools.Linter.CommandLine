using System.Collections.Generic;
using System.IO;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public interface IReportOutputFormatter
    {
        void Write(IEnumerable<RuleViolation> violations, TextWriter output);
    }
}
