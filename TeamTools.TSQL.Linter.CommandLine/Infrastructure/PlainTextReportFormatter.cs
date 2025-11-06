using System.Collections.Generic;
using System.IO;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    internal class PlainTextReportFormatter : IReportOutputFormatter
    {
        private readonly string basePath;

        public PlainTextReportFormatter(string basePath)
        {
            this.basePath = basePath;
        }

        public void Write(IEnumerable<RuleViolation> violations, TextWriter output)
        {
            foreach (var violation in violations)
            {
                // TODO : no, this path reduction has to occure elsewhere before getting to reporter
                string msg = violation.ToString();
                if (!string.IsNullOrEmpty(basePath) && msg.StartsWith(basePath, System.StringComparison.OrdinalIgnoreCase))
                {
                    msg = msg.Substring(basePath.Length).TrimStart(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                }

                output.WriteLine(msg);
            }
        }
    }
}
