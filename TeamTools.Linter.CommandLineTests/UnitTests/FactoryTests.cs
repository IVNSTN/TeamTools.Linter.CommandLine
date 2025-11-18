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
            var enumerator = factory.Make(opts, default, default, default);
            Assert.That(enumerator, Is.Not.Null);
            Assert.That(enumerator, Is.AssignableFrom(typeof(FolderFileEnumerator)));

            opts.DiffOnly = true;
            enumerator = factory.Make(opts, default, default, default);
            Assert.That(enumerator, Is.Not.Null);
            Assert.That(enumerator, Is.AssignableFrom(typeof(DiffFileEnumerator)));

            opts = new CommandLineOptions
            {
                FileName = "file",
            };
            enumerator = factory.Make(opts, default, default, default);
            Assert.That(enumerator, Is.Not.Null);
            Assert.That(enumerator, Is.AssignableFrom(typeof(SingleFileEnumerator)));
        }

        [Test]
        public void TestReporterFactoryMakes()
        {
            var opts = new CommandLineOptions
            {
                OutputFile = "file.json",
                OutputFormat = nameof(OutputFileFormat.JSON),
            };
            var reporter = ReporterFactory.Make(opts, new ConsoleReporter());

            Assert.That(reporter, Is.Not.Null);
            Assert.That(reporter, Is.AssignableFrom(typeof(FileReporter)));
        }
    }
}
