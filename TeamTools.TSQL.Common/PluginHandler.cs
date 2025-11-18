using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamTools.Common.Linting.Interfaces;
using TeamTools.Common.Linting.Properties;

namespace TeamTools.Common.Linting
{
    // TODO : some of this code can be testable
    [ExcludeFromCodeCoverage]
    public class PluginHandler : ILinterWithPluginsHandler
    {
        private const int MaxDopPerFiles = 16;
        private readonly Dictionary<string, PluginInstanceInfo> plugins = new Dictionary<string, PluginInstanceInfo>(StringComparer.OrdinalIgnoreCase);
        // TODO : shouldn't it be a ConcurrentDictionary?
        private readonly Dictionary<string, List<PluginInstanceInfo>> supportedFileTypes = new Dictionary<string, List<PluginInstanceInfo>>(StringComparer.OrdinalIgnoreCase);
        private readonly IAssemblyWrapper assemblyWrapper;

        public PluginHandler(IAssemblyWrapper assemblyWrapper)
        {
            this.assemblyWrapper = assemblyWrapper;
        }

        public void LoadPlugins(IDictionary<string, PluginInfo> pluginsWithConfig, Action<string> reportVerbose, IReporter pluginRepoter, string cultureCode = default)
        {
            foreach (var pluginConfig in pluginsWithConfig.AsParallel())
            {
                try
                {
                    reportVerbose?.Invoke(string.Format(Strings.PluginMsg_LoadingPlugin, pluginConfig.Key));
                    LoadPluginsFromAssembly(pluginConfig.Value.AssemblyPath, pluginConfig.Value.ConfigPath, pluginRepoter, reportVerbose, cultureCode);
                }
                catch (FileLoadException fe)
                {
                    pluginRepoter.ReportFailure($"Plugin {pluginConfig.Key}: {fe.Message}");
                    Environment.ExitCode = 4;
                }
                catch (Exception e)
                {
                    pluginRepoter.ReportFailure($"Failed loading plugin {pluginConfig.Key}: {e.Message}");
                    Environment.ExitCode = 2;
                }
            }
        }

        public void RunOnFileSource(string file, TextReader source, IReporter pluginRepoter)
        {
            const int MaxDopPerPlugins = 4;
            string fileName = Path.GetFileName(file);
            string fileExt = Path.GetExtension(file);

            if (!supportedFileTypes.TryGetValue(fileExt, out var relatedPlugins))
            {
                Debug.WriteLine($"No applicable plugins for file {fileName} - unsupported type");
                return;
            }

            Task.Run(() => Parallel.ForEach(
                relatedPlugins,
                new ParallelOptions { MaxDegreeOfParallelism = MaxDopPerPlugins },
                plugin => DoApplyPluginToFile(plugin.Instance, file, source, pluginRepoter))).Wait();
        }

        public void RunOnFiles(IEnumerable<string> files, Action<string> reportVerbose, IReporter pluginRepoter)
        {
            reportVerbose?.Invoke(Strings.PluginMsg_LintingBegins);
            Func<string, uint, uint> init = new Func<string, uint, uint>((_, __) => 0);
            Func<string, uint, uint> upd = new Func<string, uint, uint>((_, val) => ++val);

            var lintedFilesPerPlugin = new ConcurrentDictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
            foreach (var plugin in plugins.Keys)
            {
                lintedFilesPerPlugin.AddOrUpdate(plugin, 0, init);
            }

            var fileParallelOptions = new ParallelOptions { MaxDegreeOfParallelism = MaxDopPerFiles };
            var supportedFiles = GetFilesForRun(files, reportVerbose);

            var doRunOnFile = new Action<string>(
                file =>
                {
                    // TODO : respect basePath option here?
                    reportVerbose?.Invoke(string.Format(Strings.PluginMsg_StartingWithFile, file));

                    string fileExt = Path.GetExtension(file);

                    var pluginsForFile = supportedFileTypes[fileExt];
                    int n = pluginsForFile.Count;
                    for (int i = 0; i < n; i++)
                    {
                        var p = pluginsForFile[i];
                        DoApplyPluginToFile(p.Instance, file, pluginRepoter);
                        lintedFilesPerPlugin.AddOrUpdate(p.Name, 1, upd);
                    }
                });

            Task.Run(() => Parallel.ForEach(
                supportedFiles,
                fileParallelOptions,
                file => doRunOnFile(file))).Wait();

            if (reportVerbose != null)
            {
                reportVerbose.Invoke(Strings.PluginMsg_DoneLintingByPlugins);
                foreach (var total in lintedFilesPerPlugin)
                {
                    reportVerbose.Invoke(string.Format(Strings.PluginMsg_FilesLintedPerPlugin, total.Key, total.Value));
                }

                int totalFiles = files.Count();
                reportVerbose.Invoke(string.Format(Strings.PluginMsg_TotalFilesLinted, totalFiles));
            }
        }

        public IEnumerable<string> GetFilesForRun(IEnumerable<string> files, Action<string> reportVerbose)
        {
            return files.Where(fl => supportedFileTypes.ContainsKey(Path.GetExtension(fl)));
        }

        public bool HasPlugins() => plugins.Count > 0;

        private static void DoApplyPluginToFile(ILinter plugin, string file, IReporter reporter)
        {
            using (var reader = File.OpenText(file))
            {
                DoApplyPluginToFile(plugin, file, reader, reporter);
            }
        }

        private static void DoApplyPluginToFile(ILinter plugin, string file, TextReader reader, IReporter reporter)
        {
            var fileReporter = new SingleFileReporterDecorator(file, reporter);
            var context = new LintingContext(file, reader, fileReporter);

            plugin.PerformAction(context);
        }

        private void LoadPluginsFromAssembly(string assemblyPath, string pluginConfigPath, IReporter pluginReporter, Action<string> reportVerbose, string cultureCode)
        {
            var dll = assemblyWrapper.Load(assemblyPath);

            foreach (var type in assemblyWrapper.GetExportedTypes(dll).AsParallel())
            {
                if (!type.GetInterfaces().Contains(typeof(ILinter)))
                {
                    continue;
                }

                if (plugins.ContainsKey(type.Name))
                {
                    reportVerbose?.Invoke(string.Format(Strings.PluginMsg_PluginAlreadyLoaded, type.FullName));
                    continue;
                }

                var plugin = (ILinter)Activator.CreateInstance(type);
                plugin.Init(pluginConfigPath, pluginReporter, cultureCode);
                // rules count and supported file masks are available only after LoadConfig call
                var fileTypes = new HashSet<string>(plugin.GetSupportedFiles(), StringComparer.OrdinalIgnoreCase);

                var pluginInfo = new PluginInstanceInfo
                {
                    ClassType = type,
                    Instance = plugin,
                    Name = type.Name,
                };

                foreach (var ft in fileTypes)
                {
                    if (!supportedFileTypes.TryGetValue(ft, out var pluginsForFileExt))
                    {
                        pluginsForFileExt = new List<PluginInstanceInfo>();
                        supportedFileTypes.Add(ft, pluginsForFileExt);
                    }

                    pluginsForFileExt.Add(pluginInfo);
                }

                int rulesCount = plugin.GetRulesCount();
                plugins.Add(type.Name, pluginInfo);

                string fileTypeList = string.Join(", ", fileTypes);
                string version = assemblyWrapper.GetVersion(dll);
                reportVerbose?.Invoke(string.Format(Strings.PluginMsg_LoadedPluginInfo, type.FullName, version, rulesCount));
                reportVerbose?.Invoke(string.Format(Strings.PluginMsg_LoadedPluginSupportedFiles, fileTypeList));
            }
        }

        [ExcludeFromCodeCoverage]
        private sealed class PluginInstanceInfo
        {
            public string Name { get; set; }

            public ILinter Instance { get; set; }

            public Type ClassType { get; set; }
        }
    }
}
