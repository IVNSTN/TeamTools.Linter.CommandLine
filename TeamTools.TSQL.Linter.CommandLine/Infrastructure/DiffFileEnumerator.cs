using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class DiffFileEnumerator : FolderFileEnumerator
    {
        private readonly IVcsAccessor git;

        public DiffFileEnumerator(
            IFileSystemWrapper fileSystem,
            string rootFolder,
            string dir,
            IEnumerable<string> ignoredFolders,
            IEnumerable<string> ignoredExtensions,
            IVcsAccessor git) : base(fileSystem, rootFolder, dir, ignoredFolders, ignoredExtensions)
        {
            this.git = git;
        }

        public override IEnumerable<string> EnumFiles()
        {
            var allFiles = base.EnumFiles();
            var modifiedFiles = git.GetModifiedFiles(Dir).Select(fname => fname.Replace("/", "\\")); // git returns paths with backslash

            return allFiles.Intersect(modifiedFiles, StringComparer.OrdinalIgnoreCase);
        }
    }
}
