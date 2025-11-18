using System;
using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class DiffFileEnumerator : FolderFileEnumerator
    {
        private readonly IVcsAccessor git;
        private readonly string mainBranch;

        public DiffFileEnumerator(
            IFileSystemWrapper fileSystem,
            string rootFolder,
            string dir,
            string mainBranch,
            ICollection<string> ignoredFolders,
            ICollection<string> ignoredExtensions,
            IVcsAccessor git) : base(fileSystem, rootFolder, dir, ignoredFolders, ignoredExtensions)
        {
            this.git = git;
            this.mainBranch = mainBranch;
        }

        public override IEnumerable<string> EnumFiles()
        {
            var allFiles = base.EnumFiles();
            var modifiedFiles = git.GetModifiedFiles(Dir, mainBranch)
                .Select(fname => fname.Replace("/", "\\")); // git returns paths with backslash

            return allFiles.Intersect(modifiedFiles, StringComparer.OrdinalIgnoreCase);
        }
    }
}
