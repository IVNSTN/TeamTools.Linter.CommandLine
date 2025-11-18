using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.Common.Linting.Infrastructure;
using TeamTools.TSQL.Linter.CommandLine.Config;
using TeamTools.TSQL.Linter.CommandLine.Interfaces;

namespace TeamTools.TSQL.Linter.CommandLine.Infrastructure
{
    public class FileEnumeratorFactory
    {
        private readonly IFileSystemWrapper fileSystem;
        private readonly string rootFolder;

        public FileEnumeratorFactory(IFileSystemWrapper fileSystem, string rootFolder)
        {
            this.fileSystem = fileSystem;
            this.rootFolder = rootFolder;
        }

        public IFileEnumerator Make(
            CommandLineOptions opts,
            ICollection<string> ignoredFolders,
            ICollection<string> ignoredExtensions,
            string mainBranch)
        {
            if (!string.IsNullOrEmpty(opts.FileName))
            {
                return new SingleFileEnumerator(fileSystem, rootFolder, opts.FileName);
            }

            if (!string.IsNullOrEmpty(opts.FileListSource))
            {
                return new ListedFileEnumerator(fileSystem, rootFolder, opts.FileListSource);
            }

            if (string.IsNullOrEmpty(opts.DirectoryName))
            {
                // Default is current dir
                opts.DirectoryName = ".";
            }

            if (opts.DiffOnly)
            {
                return new DiffFileEnumerator(
                    fileSystem,
                    rootFolder,
                    opts.DirectoryName,
                    mainBranch,
                    ignoredFolders,
                    ignoredExtensions,
                    new GitDecorator(new GitCommandFactory()));
            }

            return new FolderFileEnumerator(fileSystem, rootFolder, opts.DirectoryName, ignoredFolders, ignoredExtensions);
        }
    }
}
