using System.IO;
using TeamTools.Common.Linting.Interfaces;

namespace TeamTools.Common.Linting.Infrastructure
{
    public class GitCommandFactory : IGitCommandFactory
    {
        private static readonly string GitCommandTemplate = "diff --diff-filter=MA --name-only {0} --line-prefix=\"{1}\"/ -- '{3}*.*' {2};";

        public static string GetFolderRelativePathToGitRoot(string root, string subfolder)
        {
            string sanitizedRoot = SanitizePath(root);
            string sanitizedPath = SanitizePath(subfolder);
            return PathExtension.MakeRelativePath(sanitizedRoot, sanitizedPath);
        }

        public string MakeCmdListDiffCommitted(string gitRoot, string folder, string ignoredFolders = default)
        {
            // sanitizing to prevent tail gitRoot subfolder presence in the result
            string repoSubFolder = GetFolderRelativePathToGitRoot(gitRoot, folder);
            return string.Format(GitCommandTemplate, @"--no-merges master...", gitRoot, ignoredFolders, repoSubFolder);
        }

        public string MakeCmdListDiffUncommitted(string gitRoot, string folder, string ignoredFolders = default)
        {
            // sanitizing to prevent tail gitRoot subfolder presence in the result
            string repoSubFolder = GetFolderRelativePathToGitRoot(gitRoot, folder);
            return string.Format(GitCommandTemplate, "", gitRoot, ignoredFolders, repoSubFolder);
        }

        public string MakeCmdDisableQuotePath()
        {
            return "config core.quotepath off";
        }

        public string MakeCmdShowRootPath()
        {
            return "rev-parse --show-toplevel";
        }

        private static string SanitizePath(string folderPath)
        {
            if (folderPath.EndsWith(Path.AltDirectorySeparatorChar.ToString())
            || folderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return folderPath;
            }

            return folderPath + Path.AltDirectorySeparatorChar;
        }
    }
}
