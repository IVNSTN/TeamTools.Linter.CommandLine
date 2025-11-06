using System.Collections.Generic;

namespace TeamTools.Common.Linting
{
    public interface IAppConfigLoader
    {
        IDictionary<string, PluginInfo> Plugins { get; }

        ICollection<string> IgnoredFolders { get; }

        ICollection<string> IgnoredExtensions { get; }

        void LoadFromFile(string filePath);
    }

    public struct PluginInfo
    {
        public string AssemblyPath;
        public string ConfigPath;
        public string BaseDocsPath;
        public string BaseDocsUrl;
    }
}
