// original: https://github.com/tsqllint/tsqllint/blob/main/source/TSQLLint/Program.cs
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TeamTools.Common.Linting;
using TeamTools.Common.Linting.Infrastructure;
using TeamTools.TSQL.Linter.CommandLine.Config;
using TeamTools.TSQL.Linter.CommandLine.Core;
using TeamTools.TSQL.Linter.CommandLine.Infrastructure;

namespace TeamTools.TSQL.Linter.CommandLine
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            // TODO : take output encoding from CLI params and pass to constructor
            var appReporter = new ConsoleReporter();

            try
            {
                var application = MakeApp(args, appReporter);
                application.Run();

                Task.Run(() =>
                {
                    while (NonBlockingConsole.MessageQueue.Count > 0)
                    {
                    }
                }).Wait();
            }
            catch (Exception exception)
            {
                appReporter.ReportFailure("Failed with exception:");
                ExpandException(exception, appReporter);

                Environment.ExitCode = 1;
            }
        }

        private static Application MakeApp(string[] args, IReporter appReporter)
        {
            var assembly = new AssemblyWrapper();
            var fileSystem = new FileSystemWrapper();

            return new Application(
                args,
                appReporter,
                new ConfigHandler(assembly, new AppConfigLoader(fileSystem, assembly), fileSystem),
                new PluginHandler(assembly));
        }

        private static void ExpandException(Exception err, IReporter reporter)
        {
            reporter.ReportFailure(err.Message);
            if (err is AggregateException agg)
            {
                foreach (var innerErr in agg.InnerExceptions)
                {
                    ExpandException(innerErr, reporter);
                }
            }
            else
            if (err.InnerException != null)
            {
                ExpandException(err.InnerException, reporter);
            }
        }
    }
}
