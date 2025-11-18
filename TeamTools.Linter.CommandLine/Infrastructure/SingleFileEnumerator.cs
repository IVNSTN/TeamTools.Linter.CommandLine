using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Interfaces;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class SingleFileEnumerator : IFileEnumerator
    {
        private readonly IFileSystemWrapper fileSystem;
        private readonly string rootFolder;
        private readonly string fileName;

        public SingleFileEnumerator(IFileSystemWrapper fileSystem, string rootFolder, string fileName)
        {
            this.fileSystem = fileSystem;
            this.rootFolder = rootFolder;
            this.fileName = fileName;
        }

        public IEnumerable<string> EnumFiles()
        {
            yield return fileSystem.MakeAbsolutePath(rootFolder, fileName);
        }
    }
}
