using System;
using System.Collections.Generic;

namespace TeamTools.Common.Linting.Interfaces
{
    public interface ILinterWithPluginsHandler : ILinterHandler
    {
        void LoadPlugins(IDictionary<string, PluginInfo> pluginsWithConfig, Action<string> reportVerbose, IReporter pluginRepoter, string cultureCode = default);

        bool HasPlugins();
    }
}
