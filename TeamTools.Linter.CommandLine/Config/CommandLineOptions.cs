using CommandLine;
using System;
using System.IO;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Config
{
    public class CommandLineOptions
    {
        private string configFile = SanitizePath(Path.Combine(AppContext.BaseDirectory, "DefaultConfig.json"));
        private string evaluateConfigFile = SanitizePath(Path.Combine(AppContext.BaseDirectory, "EvaluateConfig.json"));
        private string directoryName;
        private string fileName;
        private string fileListSource;
        private string outputFile;
        private string basePath;
        private OutputFileFormat outputFormat;
        private Severity minimalSeverity = Severity.Info;

        public CommandLineOptions()
        {
        }

        public OutputFileFormat Format => outputFormat;

        [Option(
            shortName: 'c',
            longName: "config",
            Required = false,
            HelpText = "Specify a path to config file with plugin list and other supported configuration options")]
        public string ConfigFile
        {
            get
            {
                return configFile;
            }

            set
            {
                configFile = string.IsNullOrEmpty(value) ? value : SanitizePath(value);
            }
        }

        [Option(
            shortName: 'd',
            longName: "dir",
            Required = false,
            HelpText = "Directory to lint. If neither --file, --filelist or --dir was provided then the default '--dir .' is used.",
            SetName = "per-dir")]
        public string DirectoryName
        {
            get
            {
                return directoryName;
            }

            set
            {
                directoryName = string.IsNullOrEmpty(value) ? value : SanitizePath(value);
            }
        }

        [Option(
            shortName: 'f',
            longName: "file",
            Required = false,
            HelpText = "Path to a single file to lint",
            SetName = "per-file")]
        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = string.IsNullOrEmpty(value) ? value : SanitizePath(value);
            }
        }

        [Option(
            shortName: 'l',
            longName: "filelist",
            Required = false,
            HelpText = "Path to a TXT file containing specific file list to lint",
            SetName = "per-list")]
        public string FileListSource
        {
            get
            {
                return fileListSource;
            }

            set
            {
                fileListSource = string.IsNullOrEmpty(value) ? value : SanitizePath(value);
            }
        }

        [Option(
            shortName: 'o',
            longName: "output",
            Default = "console",
            Required = false,
            HelpText = "Path for output file. If omitted then result is printed to stdout",
            Group = "output")]
        public string OutputFile
        {
            get
            {
                return outputFile;
            }

            set
            {
                outputFile = value.Equals("console", System.StringComparison.OrdinalIgnoreCase) ? null : value.Trim('"');
            }
        }

        [Option(
            shortName: 'm',
            longName: "format",
            Required = false,
            HelpText = "Output file format: json, sonar or plain text",
            Group = "output")]
        public string OutputFormat
        {
            get
            {
                return nameof(outputFormat);
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    outputFormat = OutputFileFormat.PlainText;
                    return;
                }

                if (value.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    outputFormat = OutputFileFormat.JSON;
                }
                else if (value.Equals("sonar", StringComparison.OrdinalIgnoreCase))
                {
                    outputFormat = OutputFileFormat.SonarQubeReport;
                }
                else
                {
                    outputFormat = OutputFileFormat.PlainText;
                }
            }
        }

        [Option(
            shortName: 's',
            longName: "severity",
            Required = false,
            HelpText = "Minimal rule severity to apply")]
        public string MinimalSeverity
        {
            get
            {
                return nameof(minimalSeverity);
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    minimalSeverity = Severity.Info;
                    return;
                }

                string input = value.Trim().ToLower();
                minimalSeverity = input switch
                {
                    "error" => Severity.Error,
                    "warning" => Severity.Warning,
                    _ => Severity.Info,
                };
            }
        }

        public Severity MinimalSeverityValue => minimalSeverity;

        [Option(
            shortName: 'r',
            longName: "basepath",
            Required = false,
            HelpText = "Root/base folder to be omitted in file paths while reporting violations",
            Group = "output")]
        public string BasePath
        {
            get
            {
                return basePath;
            }

            set
            {
                basePath = string.IsNullOrEmpty(value) ? value : SanitizePath(value);
            }
        }

        [Option(
            shortName: 'v',
            longName: "verbose",
            Required = false,
            Default = false,
            HelpText = "Verbose log")]
        public bool Verbose { get; set; }

        [Option(
            longName: "with-version",
            Required = false,
            Default = false,
            HelpText = "Print current version number before linting")]
        public bool PrintVersion { get; set; }

        [Option(
            longName: "diff",
            Required = false,
            Default = false,
            HelpText = "Run on diff files only compared to default branch",
            SetName = "per-dir")]
        public bool DiffOnly { get; set; }

        [Option(
            longName: "quiet",
            Required = false,
            Default = false,
            HelpText = "Don't set non-zero exit code if violations found")]
        public bool QuietMode { get; set; }

        [Option(
            longName: "culture",
            Required = false,
            Default = "en-us",
            HelpText = "Set culture name to switch violation messages to your language. Pass culture code as 'en-us', 'ru-ru' etc. The default is 'en-us'.")]
        public string Culture { get; set; }

        // Because AutoVersion is disabled to activate custom version info generator
        [Option(
            longName: "version",
            Required = false,
            Default = false,
            Hidden = true, // to prevent dup option info in --help screen
            HelpText = "Show current version")]
        public bool Version { get; set; }

        [Option(
            longName: "evaluate",
            Required = false,
            Default = false,
            HelpText = "Use this option for first run to detect significant violations only")]
        public bool EvaluateApp
        {
            set
            {
                if (value)
                {
                    minimalSeverity = Severity.Warning;
                    configFile = evaluateConfigFile;
                }
            }
        }

        private static string SanitizePath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            return PathExtension.MakeAbsolutePath(
                Environment.CurrentDirectory,
                PathExtension.NormalizePath(value.Trim('"', '\'')));
        }
    }
}
