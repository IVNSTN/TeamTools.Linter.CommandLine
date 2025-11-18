using CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using TeamTools.Common.Linting;
using TeamTools.Common.Linting.Interfaces;
using TeamTools.Linter.CommandLine.Properties;
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
        private readonly ConsoleReporter appReporter;
        private readonly IConfigHandler config;
        private readonly ILinterWithPluginsHandler plugins;
        private readonly string[] args;

        public Application(string[] args, ConsoleReporter appReporter, IConfigHandler config, ILinterWithPluginsHandler plugins)
        {
            timer.Start();
            this.args = args;
            this.config = config;
            this.appReporter = appReporter;
            this.plugins = plugins;
        }

        public void Run()
        {
            var opt = new CommandLineOptions();

            new Parser(cfg =>
                {
                    cfg.AutoVersion = false;
                    cfg.AutoHelp = true;
                    cfg.HelpWriter = Console.Out;
                })
                .ParseArguments<CommandLineOptions>(() => opt, args)
                .WithParsed(RunWithOptions)
                .WithNotParsed(DontRun);
        }

        private void RunWithOptions(CommandLineOptions opts)
        {
            SetCurrentCulture(opts.Culture);

            if (opts.Version)
            {
                appReporter.Report(config.GetAppVersion());
                return;
            }

            IVerboseReporter verboseReporter = null;
            if (opts.Verbose)
            {
                verboseReporter = new VerboseReporter(appReporter, opts.Verbose);
                opts.PrintVersion = true;
            }

            IReporterDecorator pluginReporter = ReporterFactory.Make(opts, appReporter);

            if (opts.MinimalSeverityValue != Severity.Info)
            {
                pluginReporter = new FilteredReporterProxy(pluginReporter, opts.MinimalSeverityValue);
            }

            try
            {
                if (opts.PrintVersion)
                {
                    appReporter.Report(string.Format(Strings.AppMessages_version_info, config.GetAppVersion()));
                }

                verboseReporter?.ReportVerbose(string.Format(Strings.AppMessages_loading_config, opts.ConfigFile));

                config.LoadFromFile(opts.ConfigFile);

                if (config.Plugins.Count == 0)
                {
                    verboseReporter?.ReportVerbose(Strings.AppErrors_bad_config);
                    return;
                }

                plugins.LoadPlugins(config.Plugins, msg => verboseReporter?.ReportVerbose(msg), pluginReporter, CultureInfo.CurrentCulture.Name);
                if (!plugins.HasPlugins())
                {
                    verboseReporter?.ReportVerbose(Strings.AppMessages_no_plugins);
                }
                else
                {
                    plugins.RunOnFiles(config.GetFilesToParse(opts), msg => verboseReporter?.ReportVerbose(msg), pluginReporter);
                }

                verboseReporter?.ReportVerbose(Strings.AppMessages_dumping_result);
                pluginReporter.ReportResults();
            }
            finally
            {
                timer.Stop();
            }

            verboseReporter?.ReportVerbose(string.Format(Strings.AppMessages_finished_in_time, timer.Elapsed.ToString()));

            if (pluginReporter.ViolationCount > 0 && !opts.QuietMode)
            {
                Environment.ExitCode = 3;
            }
        }

        private void DontRun(IEnumerable<Error> err)
        {
            var failures = err
                .Where(e => !(e is VersionRequestedError)
                    && !(e is HelpRequestedError)
                    && !(e is HelpVerbRequestedError))
                .ToList();

            if (failures.Count == 0)
            {
                // Those *RequestedError elements are no real errors
                Environment.ExitCode = 0;
                return;
            }

            var msg = new StringBuilder()
                    .Append(Strings.AppErrors_bad_cli_argument)
                    .Append(Environment.NewLine)
                    .AppendJoin(Environment.NewLine, failures.Select(f => f.Tag));

            throw new ArgumentException(msg.ToString());
        }

        private void SetCurrentCulture(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                return;
            }

            var culture = CultureInfo.GetCultureInfo(cultureCode);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }
}
