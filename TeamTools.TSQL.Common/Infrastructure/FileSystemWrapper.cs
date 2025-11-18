using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeamTools.Common.Linting.Infrastructure
{
    public class FileSystemWrapper : IFileSystemWrapper
    {
        private static readonly string DirSeparator = Path.DirectorySeparatorChar.ToString();

        public string GetFullPath(string partialPath) => Path.GetFullPath(partialPath);

        public string MakeAbsolutePath(string rootPath, string relativePath)
        {
            return PathExtension.MakeAbsolutePath(rootPath, relativePath);
        }

        public IEnumerable<string> GetAllFilesFromDirectory(string directory)
        {
            return GetAllFilesFromDirectory(directory, null, null);
        }

        public IEnumerable<string> GetAllFilesFromDirectory(string directory, ICollection<string> excludedFolders, ICollection<string> excludedFileTypes)
        {
            var folders = Directory.EnumerateDirectories(directory, "*.*", SearchOption.AllDirectories)
                .Union(Enumerable.Repeat(directory, 1));

            if (excludedFolders?.Count > 0)
            {
                // TODO : optimize filtering
                folders = folders.Where(f =>
                    !excludedFolders
                        .Any(fd =>
                            {
                                string excludedSubdir = string.Concat(DirSeparator, fd, DirSeparator);
                                return string.Concat(f, DirSeparator)
                                    .IndexOf(excludedSubdir, System.StringComparison.OrdinalIgnoreCase) >= 0;
                            }));
            }

            var files = folders.SelectMany(folder => Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly));

            if (excludedFileTypes?.Count > 0)
            {
                files = files.Where(f => !excludedFileTypes.Contains(Path.GetExtension(f)));
            }

            return files;
        }

        public TextReader OpenFile(string filePath) => File.OpenText(filePath);

        public IEnumerable<string> ReadAllLinesFromFile(string filePath) => File.ReadLines(filePath);

        public bool FileExists(string filePath) => File.Exists(filePath);
    }
}
