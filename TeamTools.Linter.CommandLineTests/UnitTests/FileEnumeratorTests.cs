using NUnit.Framework;
using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Infrastructure;
using TeamTools.TSQL.Linter.CommandLineTests.TestingInfrastructure;

namespace TeamTools.TSQL.Linter.CommandLineTests
{
    [Category("Linter.ConsoleExe")]
    public class FileEnumeratorTests
    {
        private static readonly string[] DummyFiles = new string[] { "file1", "file2" };
        private static readonly string[] DummyLines = new string[] { "line1", "line2" };
        private StubFileSystem fs;

        [SetUp]
        public void Setup()
        {
            fs = new StubFileSystem(DummyFiles, DummyLines);
        }

        [Test]
        public void TestFolderEnumerator()
        {
            var files = new FolderFileEnumerator(fs, "dummy", "dummy", null, null);
            var fileList = string.Join(";", files.EnumFiles());

            Assert.That(fileList, Is.EqualTo("file1;file2"));
        }

        [Test]
        public void TestListEnumerator()
        {
            var files = new ListedFileEnumerator(fs, "subfolder", "srcfile");
            var fileList = string.Join(";", files.EnumFiles());

            Assert.That(fileList, Is.EqualTo("subfolder\\line1;subfolder\\line2"));
        }

        [Test]
        public void TestDiffFileEnumerator()
        {
            var vcs = new StubVcs(fs.Files);
            var files = new DiffFileEnumerator(fs, "root", "folder", "main", null, null, vcs);
            var fileList = string.Join(";", files.EnumFiles());

            Assert.That(fileList, Is.EqualTo("file1;file2"));
        }

        private class StubVcs : IVcsAccessor
        {
            private readonly string[] files;

            public StubVcs(string[] files)
            {
                this.files = files;
            }

            public IEnumerable<string> GetModifiedFiles(string folder, string mainBranch) => files;
        }
    }
}
