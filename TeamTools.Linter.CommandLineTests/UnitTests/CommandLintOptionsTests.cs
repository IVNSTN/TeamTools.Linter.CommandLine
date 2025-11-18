using CommandLine;
using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.TSQL.Linter.CommandLine.Config;

namespace TeamTools.TSQL.Linter.CommandLineTests
{
    [Category("Linter.ConsoleExe")]
    public class CommandLintOptionsTests
    {
        private Dictionary<string, List<string>> argVariants;

        [SetUp]
        public void Setup()
        {
            argVariants = new Dictionary<string, List<string>>
            {
                { "scan dir", new List<string> { "--config", "c:\\conf.json", "--dir", "c:\\src" } },
                { "scan file", new List<string> { "--config", "c:\\conf.json", "--file", "c:\\src\\file.sql" } },
                { "scan diff", new List<string> { "--config", "c:\\conf.json", "--dir", "c:\\src\\proj", "--diff", "--format", "json", "--basepath", "c:\\src", "--output", "c:\\report.json", "--verbose" } },
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
            Assert.That(parsedOpts.ConfigFile, Is.EqualTo("c:\\conf.json"));
            Assert.That(parsedOpts.DirectoryName, Is.EqualTo("c:\\src\\proj"));
            Assert.That(parsedOpts.BasePath, Is.EqualTo("c:\\src"));
            Assert.That(parsedOpts.OutputFile, Is.EqualTo("c:\\report.json"));
            Assert.That(parsedOpts.Format, Is.EqualTo(OutputFileFormat.JSON));
            Assert.That(string.IsNullOrEmpty(parsedOpts.FileListSource), Is.True);
            Assert.That(string.IsNullOrEmpty(parsedOpts.FileName), Is.True);
            Assert.That(parsedOpts.DiffOnly, Is.True);
            Assert.That(parsedOpts.Verbose, Is.True);
        }
    }
}
