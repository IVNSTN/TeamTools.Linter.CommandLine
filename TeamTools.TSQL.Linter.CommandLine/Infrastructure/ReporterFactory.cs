using System.IO;
using System.Text;
using TeamTools.TSQL.Linter.CommandLine.Config;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    internal static class ReporterFactory
    {
        // TODO : less concrete output type; ConsoleReporter used because of ReportResults() method only
        public static IReporterDecorator Make(CommandLineOptions options)
        {
            if (string.IsNullOrEmpty(options.OutputFile))
            {
                // TODO : take output encoding from CLI params and pass to constructor
                return new ConsoleReporter(options.BasePath);
            }

            IReportOutputFormatter formatter = options.Format switch
            {
                OutputFileFormat.JSON => new JsonReportFormatter(options.BasePath),
                OutputFileFormat.SonarQubeReport => new SonarReportFormatter(),
                _ => new PlainTextReportFormatter(options.BasePath),
            };

            return new FileReporter(new StreamWriter(options.OutputFile, false, Encoding.UTF8), formatter);
        }
    }
}
