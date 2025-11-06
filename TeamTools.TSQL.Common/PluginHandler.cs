using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeamTools.Common.Linting.Interfaces;

namespace TeamTools.Common.Linting
{
    // TODO : some of this code can be testable
    [ExcludeFromCodeCoverage]
    public class PluginHandler : ILinterWithPluginsHandler
    {
        private const int MaxDopPerFiles = 16;
        private const int MaxDopPerPlugins = 2;
        private readonly Dictionary<Type, ILinter> plugins = new Dictionary<Type, ILinter>();
        private readonly IDictionary<Type, List<string>> pluginSupportedFiles = new Dictionary<Type, List<string>>();
        private readonly IAssemblyWrapper assemblyWrapper;

        public PluginHandler(IAssemblyWrapper assemblyWrapper)
        {
            this.assemblyWrapper = assemblyWrapper;
        }

        public IEnumerable<ILinter> Plugins => plugins.Values;

        public void LoadPlugins(IDictionary<string, PluginInfo> pluginsWithConfig, Action<string> reportVerbose, IReporter pluginRepoter, string cultureCode = default)
        {
            foreach (var pluginConfig in pluginsWithConfig)
            {
                try
                {
                    reportVerbose?.Invoke($"Loading {pluginConfig.Key} plugin...");
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
            string fileName = Path.GetFileName(file);

            Task.Run(() => Parallel.ForEach(
                plugins.Keys,
                new ParallelOptions { MaxDegreeOfParallelism = MaxDopPerPlugins },
                plugin =>
                {
                    if (pluginSupportedFiles[plugin].Exists(mask => fileName.EndsWith(mask, StringComparison.OrdinalIgnoreCase)))
                    {
                        DoApplyPluginToFile(plugins[plugin], file, source, pluginRepoter);
                    }
                    else
                    {
                        Debug.WriteLine($"Skipping plugin {plugin} for file {fileName} - unsupported type");
                    }
                })).Wait();
        }

        public void RunOnFiles(IEnumerable<string> files, Action<string> reportVerbose, IReporter pluginRepoter)
        {
            reportVerbose?.Invoke("Linting...");
            int totalFiles = 0;
            var lintedFilesPerPlugin = new ConcurrentDictionary<string, uint>(StringComparer.OrdinalIgnoreCase);

            foreach (var plugin in plugins.Keys)
            {
                lintedFilesPerPlugin.AddOrUpdate(plugin.Name, 0, (key, val) => 0);
            }

            Task.Run(() => Parallel.ForEach(
                files,
                new ParallelOptions { MaxDegreeOfParallelism = MaxDopPerFiles },
                file =>
                {
                    totalFiles++;
                    string fileName = Path.GetFileName(file);
                    reportVerbose?.Invoke($"starting with file: {fileName}");

                    Task.Run(() => Parallel.ForEach(
                        plugins.Keys,
                        new ParallelOptions { MaxDegreeOfParallelism = MaxDopPerPlugins },
                        plugin =>
                        {
                            if (pluginSupportedFiles[plugin].Exists(mask => fileName.EndsWith(mask, StringComparison.OrdinalIgnoreCase)))
                            {
                                DoApplyPluginToFile(plugins[plugin], file, pluginRepoter);
                                lintedFilesPerPlugin.AddOrUpdate(plugin.Name, 1, (key, val) => ++val);
                            }
                            else
                            {
                                Debug.WriteLine($"Skipping plugin {plugin} for file {fileName} - unsupported type");
                            }
                        })).Wait();
                })).Wait();

            if (reportVerbose != null)
            {
                reportVerbose.Invoke("Done linting files by plugins:");
                foreach (var total in lintedFilesPerPlugin)
                {
                    reportVerbose.Invoke($"    {total.Key} - {total.Value} files");
                }

                reportVerbose.Invoke($"    out of total {totalFiles} files.");
            }
        }

        public bool HasPlugins()
        {
            return Plugins.Any();
        }

        private void DoApplyPluginToFile(ILinter plugin, string file, IReporter reporter)
        {
            using (var reader = new StreamReader(file))
            {
                var fileReporter = new SingleFileReporterDecorator(file, reporter);
                var context = new LintingContext(file, reader, fileReporter);

                plugin.PerformAction(context);
            }
        }

        private void DoApplyPluginToFile(ILinter plugin, string file, TextReader reader, IReporter reporter)
        {
            var fileReporter = new SingleFileReporterDecorator(file, reporter);
            var context = new LintingContext(file, reader, fileReporter);

            plugin.PerformAction(context);
        }

        private void LoadPluginsFromAssembly(string assemblyPath, string pluginConfigPath, IReporter pluginReporter, Action<string> reportVerbose, string cultureCode)
        {
            var dll = assemblyWrapper.Load(assemblyPath);

            foreach (var type in assemblyWrapper.GetExportedTypes(dll))
            {
                var interfaces = type.GetInterfaces().ToList();

                if (!interfaces.Contains(typeof(ILinter)))
                {
                    continue;
                }

                if (plugins.ContainsKey(type))
                {
                    reportVerbose?.Invoke($"Already loaded plugin '{type.FullName}'");
                    continue;
                }

                var plugin = (ILinter)Activator.CreateInstance(type);
                plugin.Init(pluginConfigPath, pluginReporter, cultureCode);
                // rules count and supported file masks are available only after LoadConfig call
                var fileTypes = plugin.GetSupportedFiles().ToList();
                pluginSupportedFiles.Add(type, fileTypes);
                int rulesCount = plugin.GetRulesCount();
                plugins.Add(type, plugin);

                string fileTypeList = string.Join(", ", fileTypes);
                string version = assemblyWrapper.GetVersion(dll);
                reportVerbose?.Invoke($"Loaded plugin: '{type.FullName}', Version: '{version}', Rules: {rulesCount}");
                reportVerbose?.Invoke($"    for files: {fileTypeList}");
            }
        }
    }
}
