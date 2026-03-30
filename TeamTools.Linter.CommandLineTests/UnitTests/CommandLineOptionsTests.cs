using CommandLine;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using TeamTools.TSQL.Linter.CommandLine.Config;

namespace TeamTools.TSQL.Linter.CommandLineTests
{
    [Category("Linter.ConsoleExe")]
    public class CommandLineOptionsTests
    {
#if Windows
        private const string BasePath = @"c:\";
#else
        private const string BasePath = @"/home/";
#endif

        private Dictionary<string, List<string>> argVariants;

        [SetUp]
        public void Setup()
        {
            argVariants = new Dictionary<string, List<string>>
            {
                { "scan dir", new List<string> { "--config", BasePath + "conf.json", "--dir", BasePath + "src" } },
                { "scan file", new List<string> { "--config", BasePath + "conf.json", "--file", Path.Combine(BasePath, "src", "file.sql") } },
                { "scan diff", new List<string> { "--config", BasePath + "conf.json", "--dir", Path.Combine(BasePath, "src", "proj"), "--diff", "--format", "json", "--basepath", BasePath + "src", "--output", BasePath + "report.json", "--verbose" } },
            };
        }

        [Test]
        public void TestBasicArgumentSetParsedWell()
        {
            foreach (var args in argVariants)
            {
                Parser.Default.ParseArguments<CommandLineOptions>(args.Value)
                    .WithNotParsed(err => Assert.Fail(args.Key + ": " + string.Join(";", err)));
            }

            Assert.Pass();
        }

        [Test]
        public void TestAllArgumentsExtractedCorrectly()
        {
            CommandLineOptions parsedOpts = null;
            Parser.Default.ParseArguments<CommandLineOptions>(argVariants["scan diff"])
                .WithParsed(opts => parsedOpts = opts)
                .WithNotParsed(err => Assert.Fail(string.Join(";", err)));

            Assert.That(parsedOpts, Is.Not.Null);
            Assert.That(parsedOpts.ConfigFile, Is.EqualTo(BasePath + "conf.json"));
            Assert.That(parsedOpts.DirectoryName, Is.EqualTo(Path.Combine(BasePath, "src", "proj")));
            Assert.That(parsedOpts.BasePath, Is.EqualTo(BasePath + "src"));
            Assert.That(parsedOpts.OutputFile, Is.EqualTo(BasePath + "report.json"));
            Assert.That(parsedOpts.Format, Is.EqualTo(OutputFileFormat.JSON));
            Assert.That(string.IsNullOrEmpty(parsedOpts.FileListSource), Is.True);
            Assert.That(string.IsNullOrEmpty(parsedOpts.FileName), Is.True);
            Assert.That(parsedOpts.DiffOnly, Is.True);
            Assert.That(parsedOpts.Verbose, Is.True);
        }
    }
}
