using System.Collections.Generic;
using System.IO;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLineTests.TestingInfrastructure
{
    public class StubFileSystem : IFileSystemWrapper
    {
        private readonly string[] files;
        private readonly string[] lines;

        public StubFileSystem(string[] files)
        {
            this.files = files;
        }

        public StubFileSystem(string[] files, string[] lines)
        {
            this.files = files;
            this.lines = lines;
        }

        public string[] Files => files;

        public string GetFullPath(string partialPath)
        {
            return partialPath;
        }

        public IEnumerable<string> GetAllFilesFromDirectory(string directory)
        {
            return files;
        }

        public IEnumerable<string> GetAllFilesFromDirectory(string directory, IEnumerable<string> excludedFolders, IEnumerable<string> excludedFileTypes)
        {
            return files;
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
            return new StringReader(string.Join("\r\n", lines));
        }

        public string MakeAbsolutePath(string rootPath, string relativePath)
        {
            return Path.Combine(rootPath, relativePath);
        }
    }
}
