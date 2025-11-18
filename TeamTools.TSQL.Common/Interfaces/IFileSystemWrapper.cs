using System.Collections.Generic;
using System.IO;

namespace TeamTools.Common.Linting
{
    public interface IFileSystemWrapper
    {
        string GetFullPath(string partialPath);

        IEnumerable<string> GetAllFilesFromDirectory(string directory);

        IEnumerable<string> GetAllFilesFromDirectory(string directory, ICollection<string> excludedFolders, ICollection<string> excludedFileTypes);

        IEnumerable<string> ReadAllLinesFromFile(string filePath);

        TextReader OpenFile(string filePath);

        bool FileExists(string filePath);

        string MakeAbsolutePath(string rootPath, string relativePath);
    }
}
