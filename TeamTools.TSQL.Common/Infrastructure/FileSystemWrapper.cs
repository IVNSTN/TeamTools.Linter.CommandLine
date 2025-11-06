using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeamTools.Common.Linting.Infrastructure
{
    public class FileSystemWrapper : IFileSystemWrapper
    {
        public string GetFullPath(string partialPath) => Path.GetFullPath(partialPath);

        public string MakeAbsolutePath(string rootPath, string relativePath)
        {
            return PathExtension.MakeAbsolutePath(rootPath, relativePath);
        }

        public IEnumerable<string> GetAllFilesFromDirectory(string directory)
        {
            return GetAllFilesFromDirectory(directory, null, null);
        }

        public IEnumerable<string> GetAllFilesFromDirectory(string directory, IEnumerable<string> excludedFolders, IEnumerable<string> excludedFileTypes)
        {
            var folders = Directory.EnumerateDirectories(directory, "*.*", SearchOption.AllDirectories).ToList();
            folders.Add(directory);

            if (excludedFolders != null && excludedFolders.Any())
            {
                folders = folders.Where(f =>
                    !excludedFolders
                        .Any(fd =>
                            {
                                string excludedSubdir = string.Concat(Path.DirectorySeparatorChar, fd, Path.DirectorySeparatorChar);
                                return string.Concat(f, Path.DirectorySeparatorChar)
                                    .IndexOf(excludedSubdir, System.StringComparison.OrdinalIgnoreCase) >= 0;
                            }))
                    .ToList();
            }

            var files = folders.SelectMany(folder => Directory.EnumerateFiles(folder, "*.*", SearchOption.TopDirectoryOnly));

            if (excludedFileTypes != null && excludedFileTypes.Any())
            {
                files = files.Where(f => !excludedFileTypes.Any(ft => f.EndsWith(ft, System.StringComparison.OrdinalIgnoreCase)));
            }

            return files;
        }

        public TextReader OpenFile(string filePath) => new StreamReader(filePath);

        public IEnumerable<string> ReadAllLinesFromFile(string filePath) => File.ReadLines(filePath);

        public bool FileExists(string filePath) => File.Exists(filePath);
    }
}
