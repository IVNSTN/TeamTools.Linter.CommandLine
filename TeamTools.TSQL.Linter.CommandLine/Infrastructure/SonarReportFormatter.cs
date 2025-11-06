using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    [ExcludeFromCodeCoverage]
    internal class SonarReportFormatter : IReportOutputFormatter
    {
        public void Write(IEnumerable<RuleViolation> violations, TextWriter output)
        {
            throw new NotImplementedException();
        }
    }
}
