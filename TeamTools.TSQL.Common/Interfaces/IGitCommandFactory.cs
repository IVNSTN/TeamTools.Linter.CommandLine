namespace TeamTools.Common.Linting.Interfaces
{
    public interface IGitCommandFactory
    {
        string MakeCmdListDiffCommitted(string gitRoot, string folder, string ignoredFolders = default);

        string MakeCmdListDiffUncommitted(string gitRoot, string folder, string ignoredFolders = default);

        string MakeCmdDisableQuotePath();

        string MakeCmdShowRootPath();
    }
}
