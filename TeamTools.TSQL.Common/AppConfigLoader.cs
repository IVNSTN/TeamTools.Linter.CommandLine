using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeamTools.Common.Linting
{
    public class AppConfigLoader : IAppConfigLoader
    {
        private const string PluginListConfigNode = "plugins";
        private const string IgnoredFoldersConfigNode = "ignore.folders";
        private const string IgnoredFileExtensionsConfigNode = "ignore.extensions";
        private readonly IFileSystemWrapper fileSystem;
        private string rootPath;

        public AppConfigLoader(IFileSystemWrapper fileSystem, IAssemblyWrapper assembly)
        {
            this.fileSystem = fileSystem;
            this.rootPath = assembly.GetExecutingPath();
        }

        public IDictionary<string, PluginInfo> Plugins { get; } = new Dictionary<string, PluginInfo>(StringComparer.OrdinalIgnoreCase);

        public ICollection<string> IgnoredFolders { get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public ICollection<string> IgnoredExtensions { get; } = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        public void Load(TextReader config)
        {
            Plugins.Clear();

            if (TryParseJson(config.ReadToEnd(), out JToken json))
            {
                ExtractPlugins(json);
                ExtractIgnores(json);
            }
            else
            {
                throw new FormatException("Config is not a valid JSON");
            }
        }

        public void LoadFromFile(string filePath)
        {
            if (!fileSystem.FileExists(filePath))
            {
                throw new FileNotFoundException("Config file not found: " + filePath);
            }

            // switching to relative to config file path
            rootPath = Path.GetDirectoryName(filePath);

            using (var src = fileSystem.OpenFile(filePath))
            {
                Load(src);
            }
        }

        private static bool TryParseJson(string jsonString, out JToken token)
        {
            try
            {
                token = JToken.Parse(jsonString);
                return true;
            }
            catch
            {
                token = null;
                return false;
            }
        }

        private void ExtractPlugins(JToken jsonObject)
        {
            var plugins = jsonObject.SelectTokens(".." + PluginListConfigNode).ToList();

            foreach (var plugin in plugins)
            {
                foreach (var jsonToken in plugin.Children())
                {
                    var prop = (JProperty)jsonToken;

                    var pluginOptions = prop.Value.ToObject<Dictionary<string, string>>();

                    pluginOptions.TryGetValue("docsBasePath", out string docsBasePath);
                    pluginOptions.TryGetValue("docsBaseUrl", out string docsBaseUrl);

                    if (pluginOptions.TryGetValue("dll", out string pluginPath)
                    && pluginOptions.TryGetValue("config", out string pluginConfigPath))
                    {
                        Plugins.Add(
                            prop.Name,
                            new PluginInfo
                            {
                                AssemblyPath = fileSystem.MakeAbsolutePath(rootPath, pluginPath),
                                ConfigPath = fileSystem.MakeAbsolutePath(rootPath, pluginConfigPath),
                                BaseDocsPath = fileSystem.MakeAbsolutePath(rootPath, docsBasePath),
                                BaseDocsUrl = docsBaseUrl,
                            });
                    }
                }
            }
        }

        private void ExtractIgnores(JToken jsonObject)
        {
            FillIgnoreList(IgnoredFolders, jsonObject, ".." + IgnoredFoldersConfigNode);
            FillIgnoreList(IgnoredExtensions, jsonObject, ".." + IgnoredFileExtensionsConfigNode);
        }

        private void FillIgnoreList(ICollection<string> ignoreCollection, JToken jsonObject, string path)
        {
            ignoreCollection.Clear();
            foreach (string value in ExtractListFromJson(jsonObject, path))
            {
                ignoreCollection.Add(value);
            }
        }

        private IEnumerable<string> ExtractListFromJson(JToken jsonObject, string path)
        {
            return jsonObject
                .SelectTokens(path)
                .ToList()
                .ToList()
                .Children()
                .Select(jt => ((JValue)jt).Value.ToString())
                .Distinct();
        }
    }
}
