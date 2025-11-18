using System.Collections.Generic;
using System.Linq;
using TeamTools.Common.Linting.Interfaces;

namespace TeamTools.Common.Linting
{
    public class GitDecorator : IVcsAccessor
    {
        private readonly IGitCommandFactory cmdFactory;
        private readonly string ignoredFolders;

        public GitDecorator(IGitCommandFactory cmdFactory, string ignoredFolders = default)
        {
            this.cmdFactory = cmdFactory;
            this.ignoredFolders = ignoredFolders ?? "";

            if (!string.IsNullOrEmpty(ignoredFolders))
            {
                // turning into `git diff` ignore folder syntax
                // TODO : via cmdFactory?
                this.ignoredFolders = string.Join(" ", this.ignoredFolders.Split(';', ',')
                    .Select(f => string.Format("':(exclude,top){0}'", f)));
            }
        }

        /* TODO : for async implementation
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        } */

        public IEnumerable<string> GetModifiedFiles(string folder, string mainBranch)
        {
            var gitChanges = new GitOutputLogger();
            var git = new GitProcessRunner(gitChanges, folder);

            git.ExecuteCmd(cmdFactory.MakeCmdDisableQuotePath());

            string gitRoot = GetRootPath(git, gitChanges);

            gitChanges.Reset();
            // TODO : async
            git.ExecuteCmd(cmdFactory.MakeCmdListDiffUncommitted(gitRoot, folder, ignoredFolders));
            git.ExecuteCmd(cmdFactory.MakeCmdListDiffCommitted(gitRoot, folder, mainBranch, ignoredFolders));

            return gitChanges.Output
                .Where(msg => !string.IsNullOrWhiteSpace(msg))
                .OrderBy(msg => msg)
                .Select(msg => PathExtension.MakeAbsolutePath(folder, msg));
        }

        private string GetRootPath(GitProcessRunner git, GitOutputLogger gitOutput)
        {
            gitOutput.Reset();
            git.ExecuteCmd(cmdFactory.MakeCmdShowRootPath());
            return gitOutput.Output.FirstOrDefault();
        }

        private class GitOutputLogger : ITextOutputPort
        {
            public List<string> Output { get; } = new List<string>();

            public void WriteLine(string message)
            {
                Output.Add(message);
            }

            public void Reset()
            {
                Output.Clear();
            }
        }
    }
}
