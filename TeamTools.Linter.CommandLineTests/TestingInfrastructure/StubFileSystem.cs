using System;
using System.Collections.Generic;
using System.IO;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLineTests.TestingInfrastructure
{
    public class StubFileSystem : IFileSystemWrapper
    {
        private readonly string[] lines;

        public StubFileSystem(string[] files)
        {
            this.Files = files;
        }

        public StubFileSystem(string[] files, string[] lines)
        {
            this.Files = files;
            this.lines = lines;
        }

        public string[] Files { get; }

        public string GetFullPath(string partialPath)
        {
            return partialPath;
        }

        public IEnumerable<string> GetAllFilesFromDirectory(string directory)
        {
            return Files;
        }

        public IEnumerable<string> GetAllFilesFromDirectory(string directory, ICollection<string> excludedFolders, ICollection<string> excludedFileTypes)
        {
            return Files;
        }

        public IEnumerable<string> ReadAllLinesFromFile(string filePath)
        {
            return lines;
        }

        public bool FileExists(string filePath)
        {
            return true;
        }

        public TextReader OpenFile(string filePath)
        {
            return new StringReader(string.Join(Environment.NewLine, lines));
        }

        public string MakeAbsolutePath(string rootPath, string relativePath)
        {
            return Path.Combine(rootPath, relativePath);
        }
    }
}
