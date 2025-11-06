using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Interfaces;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class FolderFileEnumerator : IFileEnumerator
    {
        private readonly IFileSystemWrapper fileSystem;
        private readonly string rootFolder;
        private readonly string dir;
        private readonly IEnumerable<string> ignoredFolders;
        private readonly IEnumerable<string> ignoredExtensions;

        public FolderFileEnumerator(
            IFileSystemWrapper fileSystem,
            string rootFolder,
            string dir,
            IEnumerable<string> ignoredFolders,
            IEnumerable<string> ignoredExtensions)
        {
            this.fileSystem = fileSystem;
            this.rootFolder = rootFolder;
            this.dir = dir;
            this.ignoredFolders = ignoredFolders;
            this.ignoredExtensions = ignoredExtensions;
        }

        protected string Dir => dir;

        public virtual IEnumerable<string> EnumFiles()
        {
            var files = fileSystem.GetAllFilesFromDirectory(
                fileSystem.MakeAbsolutePath(rootFolder, dir),
                ignoredFolders,
                ignoredExtensions);

            return files;
        }
    }
}
