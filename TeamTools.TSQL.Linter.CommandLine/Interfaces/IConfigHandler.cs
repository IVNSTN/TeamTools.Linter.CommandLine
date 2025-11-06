using System.Collections.Generic;
using TeamTools.Common.Linting;
using TeamTools.TSQL.Linter.CommandLine.Config;

namespace TeamTools.TSQL.Linter.CommandLine.Interfaces
{
    internal interface IConfigHandler
    {
        IDictionary<string, PluginInfo> Plugins { get; }

        void LoadFromFile(string filePath);

        string GetAppVersion();

        IEnumerable<string> GetFilesToParse(CommandLineOptions opts);
    }
}
