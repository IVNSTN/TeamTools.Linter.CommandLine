using NUnit.Framework;
using TeamTools.TSQL.Linter.CommandLine.Config;
using TeamTools.TSQL.Linter.CommandLine.Infrastructure;
using TeamTools.TSQL.Linter.CommandLineTests.TestingInfrastructure;

namespace TeamTools.TSQL.Linter.CommandLineTests
{
    [Category("Linter.ConsoleExe")]
    public class FactoryTests
    {
        [Test]
        public void TestFileEnumeratorFactoryMakes()
        {
            var fs = new StubFileSystem(null, null);
            var factory = new FileEnumeratorFactory(fs, @"c:\root");

            var opts = new CommandLineOptions
            {
                DirectoryName = "dir",
            };
            var enumerator = factory.Make(opts, null, null);
            Assert.That(enumerator, Is.Not.Null);
            Assert.IsAssignableFrom(typeof(FolderFileEnumerator), enumerator);

            opts.DiffOnly = true;
            enumerator = factory.Make(opts, null, null);
            Assert.That(enumerator, Is.Not.Null);
            Assert.IsAssignableFrom(typeof(DiffFileEnumerator), enumerator);

            opts = new CommandLineOptions
            {
                FileName = "file",
            };
            enumerator = factory.Make(opts, null, null);
            Assert.That(enumerator, Is.Not.Null);
            Assert.IsAssignableFrom(typeof(SingleFileEnumerator), enumerator);
        }

        [Test]
        public void TestReporterFactoryMakes()
        {
            var opts = new CommandLineOptions
            {
                OutputFile = "file.json",
                OutputFormat = nameof(OutputFileFormat.JSON),
            };
            var reporter = ReporterFactory.Make(opts);

            Assert.That(reporter, Is.Not.Null);
            Assert.IsAssignableFrom(typeof(FileReporter), reporter);
        }
    }
}
