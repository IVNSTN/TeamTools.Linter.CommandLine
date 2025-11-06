using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TeamTools.Common.Linting;
using TeamTools.Common.Linting.Interfaces;
using TeamTools.TSQL.Linter.CommandLine.Config;
using TeamTools.TSQL.Linter.CommandLine.Infrastructure;
using TeamTools.TSQL.Linter.CommandLine.Interfaces;

namespace TeamTools.TSQL.Linter.CommandLine.Core
{
    // TODO : at least some of this code can be testable
    [ExcludeFromCodeCoverage]
    internal sealed class Application
    {
        private readonly Stopwatch timer = new Stopwatch();
        private readonly IReporter appReporter;
        private readonly IConfigHandler config;
        private readonly ILinterWithPluginsHandler plugins;
        private readonly string[] args;

        public Application(string[] args, IReporter appReporter, IConfigHandler config, ILinterWithPluginsHandler plugins)
        {
            timer.Start();
            this.args = args;
            this.config = config;
            this.appReporter = appReporter;
            this.plugins = plugins;
        }

        public void Run()
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(RunWithOptions)
                .WithNotParsed(DontRun);
        }

        private void RunWithOptions(CommandLineOptions opts)
        {
            IVerboseReporter verboseReporter = null;
            if (opts.Verbose)
            {
                verboseReporter = new VerboseReporter(appReporter, opts.Verbose);
                opts.Version = true;
            }

            IReporterDecorator pluginReporter = ReporterFactory.Make(opts);

            if (opts.MinimalSeverityValue != Severity.Info)
            {
                pluginReporter = new FilteredReporterProxy(pluginReporter, opts.MinimalSeverityValue);
            }

            try
            {
                if (opts.Version)
                {
                    appReporter.Report("Version: " + config.GetAppVersion());
                }

                verboseReporter?.ReportVerbose("Loading config from " + opts.ConfigFile);

                config.LoadFromFile(opts.ConfigFile);

                if (config.Plugins.Count == 0)
                {
                    verboseReporter?.ReportVerbose("No plugins to load or invalid config format. Doing nothing");
                    return;
                }

                plugins.LoadPlugins(config.Plugins, msg => verboseReporter?.ReportVerbose(msg), pluginReporter);
                if (!plugins.HasPlugins())
                {
                    verboseReporter?.ReportVerbose("No plugins loaded. Doing nothing.");
                }
                else
                {
                    plugins.RunOnFiles(config.GetFilesToParse(opts), msg => verboseReporter?.ReportVerbose(msg), pluginReporter);
                }

                verboseReporter?.ReportVerbose("dumping result...");
                pluginReporter.ReportResults();
            }
            finally
            {
                timer.Stop();
            }

            verboseReporter?.ReportVerbose(string.Format("Finished in {0}", timer.Elapsed.ToString()));

            if (pluginReporter.ViolationCount > 0 && !opts.QuietMode)
            {
                Environment.ExitCode = 3;
            }
        }

        private void DontRun(IEnumerable<Error> err)
        {
            var failures = err.Where(e => !(e is VersionRequestedError) && !(e is HelpRequestedError)).ToList();

            if (failures.Count == 0)
            {
                return;
            }

            throw new ArgumentException(
                @"Wrong command-line arguments. Use --help to see valid arguments." + Environment.NewLine +
                string.Join(Environment.NewLine, failures.Select(f => f.Tag)));
        }
    }
}
