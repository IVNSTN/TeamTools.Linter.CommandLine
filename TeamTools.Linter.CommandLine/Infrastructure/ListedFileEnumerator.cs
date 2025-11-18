using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Interfaces;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class ListedFileEnumerator : IFileEnumerator
    {
        private readonly IFileSystemWrapper fileSystem;
        private readonly string rootFolder;
        private readonly string srcFileName;

        public ListedFileEnumerator(IFileSystemWrapper fileSystem, string rootFolder, string srcFileName)
        {
            this.fileSystem = fileSystem;
            this.rootFolder = rootFolder;
            this.srcFileName = srcFileName;
        }

        public IEnumerable<string> EnumFiles()
        {
            foreach (var fileName in fileSystem.ReadAllLinesFromFile(fileSystem.MakeAbsolutePath(rootFolder, srcFileName)))
            {
                if (!string.IsNullOrWhiteSpace(fileName))
                {
                    yield return fileSystem.MakeAbsolutePath(rootFolder, fileName.Trim());
                }
            }
        }
    }
}
