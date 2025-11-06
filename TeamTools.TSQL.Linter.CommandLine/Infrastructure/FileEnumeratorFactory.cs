using System;
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

        public IFileEnumerator Make(CommandLineOptions opts, IEnumerable<string> ignoredFolders, IEnumerable<string> ignoredExtensions)
        {
            if (!string.IsNullOrEmpty(opts.FileName))
            {
                return new SingleFileEnumerator(fileSystem, rootFolder, opts.FileName);
            }
            else if (!string.IsNullOrEmpty(opts.DirectoryName))
            {
                if (opts.DiffOnly)
                {
                    return new DiffFileEnumerator(
                        fileSystem,
                        rootFolder,
                        opts.DirectoryName,
                        ignoredFolders,
                        ignoredExtensions,
                        new GitDecorator(new GitCommandFactory()));
                }

                return new FolderFileEnumerator(fileSystem, rootFolder, opts.DirectoryName, ignoredFolders, ignoredExtensions);
            }
            else if (!string.IsNullOrEmpty(opts.FileListSource))
            {
                return new ListedFileEnumerator(fileSystem, rootFolder, opts.FileListSource);
            }

            throw new InvalidOperationException("Undefined file list source for linting");
        }
    }
}
