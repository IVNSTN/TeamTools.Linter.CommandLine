using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Interfaces;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class FolderFileEnumerator : IFileEnumerator
    {
        private readonly IFileSystemWrapper fileSystem;
        private readonly string rootFolder;
        private readonly ICollection<string> ignoredFolders;
        private readonly ICollection<string> ignoredExtensions;

        public FolderFileEnumerator(
            IFileSystemWrapper fileSystem,
            string rootFolder,
            string dir,
            ICollection<string> ignoredFolders,
            ICollection<string> ignoredExtensions)
        {
            this.fileSystem = fileSystem;
            this.rootFolder = rootFolder;
            this.Dir = dir;
            this.ignoredFolders = ignoredFolders;
            this.ignoredExtensions = ignoredExtensions;
        }

        protected string Dir { get; }

        public virtual IEnumerable<string> EnumFiles()
        {
            var files = fileSystem.GetAllFilesFromDirectory(
                fileSystem.MakeAbsolutePath(rootFolder, Dir),
                ignoredFolders,
                ignoredExtensions);

            return files;
        }
    }
}
