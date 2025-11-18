using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace TeamTools.Common.Linting
{
    [ExcludeFromCodeCoverage]
    public static class PathExtension
    {
        private static readonly string[] Prefixes = new string[] { @"\\", @"//", @"file://" };

        public static string NormalizePath(string path)
        {
            return path
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        public static string ResolvePath(string path)
        {
            return NormalizePath(Path.GetFullPath(new Uri(path, UriKind.Absolute).LocalPath));
        }

        public static bool IsFullyQualified(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            if (Prefixes.Any(s => path.StartsWith(s)))
            {
                return true;
            }

            if (Path.IsPathRooted(path))
            {
                return true;
            }

            return false;
        }

        public static string MakeAbsolutePath(string root, string full)
        {
            if (string.IsNullOrEmpty(full)
            || string.Equals(full, Path.DirectorySeparatorChar)
            || string.Equals(full, Path.AltDirectorySeparatorChar))
            {
                return root;
            }

            if (IsFullyQualified(full))
            {
                return full;
            }

            return ResolvePath(Path.Combine(root, full));
        }

        // root should end with directory separator if don't want to see
        // the tail subfolder in the result
        public static string MakeRelativePath(string root, string full)
        {
            var rootPath = new Uri(root, UriKind.Absolute);
            var fullPath = new Uri(full, UriKind.Absolute);

            if (rootPath.Scheme != fullPath.Scheme)
            {
                return full;
            }

            var rltvPath = rootPath.MakeRelativeUri(fullPath).ToString();
            if (string.IsNullOrEmpty(rltvPath))
            {
                return rltvPath;
            }

            return Uri.UnescapeDataString(rltvPath)
                .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
