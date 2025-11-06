using CommandLine;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Config
{
    public class CommandLineOptions
    {
        private string configFile;
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
            Required = true,
            HelpText = "Specify a path to config file with plugin list")]
        public string ConfigFile
        {
            get
            {
                return configFile;
            }

            set
            {
                configFile = SanitizePath(value);
            }
        }

        [Option(
            shortName: 'd',
            longName: "dir",
            Required = false,
            HelpText = "Directory to lint",
            SetName = "per-dir")]
        public string DirectoryName
        {
            get
            {
                return directoryName;
            }

            set
            {
                directoryName = SanitizePath(value);
            }
        }

        [Option(
            shortName: 'f',
            longName: "file",
            Required = false,
            HelpText = "Single file to lint",
            SetName = "per-file")]
        public string FileName
        {
            get
            {
                return fileName;
            }

            set
            {
                fileName = SanitizePath(value);
            }
        }

        [Option(
            shortName: 'l',
            longName: "filelist",
            Required = false,
            HelpText = "Path to a TXT file with file list to lint inside",
            SetName = "per-list")]
        public string FileListSource
        {
            get
            {
                return fileListSource;
            }

            set
            {
                fileListSource = SanitizePath(value);
            }
        }

        [Option(
            shortName: 'o',
            longName: "output",
            Default = "console",
            Required = false,
            HelpText = "Path to an output file",
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
            HelpText = "Output file format: json, sonar or text",
            Group = "output")]
        public string OutputFormat
        {
            get
            {
                return nameof(outputFormat);
            }

            set
            {
                string input = value.Trim().ToLower();
                outputFormat = input switch
                {
                    "json" => OutputFileFormat.JSON,
                    "sonar" => OutputFileFormat.SonarQubeReport,
                    _ => OutputFileFormat.PlainText,
                };
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
            HelpText = "Root/base folder to be omitted in file paths in reports",
            Group = "output")]
        public string BasePath
        {
            get
            {
                return basePath;
            }

            set
            {
                basePath = SanitizePath(value);
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
            shortName: 'n',
            longName: "withversion",
            Required = false,
            Default = false,
            HelpText = "Print current version number before linting")]
        public bool Version { get; set; }

        [Option(
            longName: "diff",
            Required = false,
            Default = false,
            HelpText = "Run on diff files only compared to master branch",
            SetName = "per-dir")]
        public bool DiffOnly { get; set; }

        [Option(
            longName: "quiet",
            Required = false,
            Default = false,
            HelpText = "Don't set non-zero exit code if violations found")]
        public bool QuietMode { get; set; }

        private static string SanitizePath(string value)
        {
            return PathExtension.NormalizePath(value.Trim('"', '\''));
        }
    }
}
