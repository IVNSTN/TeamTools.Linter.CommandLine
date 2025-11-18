using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TeamTools.Common.Linting
{
    // TODO : It does not really look like a _very_common_ thing
    // and probably should be moved elsewhere from the current project.
    public class AppConfigLoader : IAppConfigLoader
    {
        private const string PluginListConfigNode = "plugins";
        private const string IgnoredFoldersConfigNode = "ignore.folders";
        private const string IgnoredFileExtensionsConfigNode = "ignore.extensions";
        private const string OptionsConfigNode = "options";
        private readonly IFileSystemWrapper fileSystem;
        private string rootPath;

        public AppConfigLoader(IFileSystemWrapper fileSystem, IAssemblyWrapper assembly)
        {
            this.fileSystem = fileSystem;
            this.rootPath = assembly.GetExecutingPath();
        }

        public IDictionary<string, PluginInfo> Plugins { get; } = new Dictionary<string, PluginInfo>(StringComparer.OrdinalIgnoreCase);

        public ICollection<string> IgnoredFolders { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ICollection<string> IgnoredExtensions { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public string MainBranch { get; private set; } = "master";

        public void Load(TextReader config)
        {
            if (TryParseJson(config.ReadToEnd(), out JToken json))
            {
                ExtractPlugins(json);
                ExtractIgnores(json);
                ExtractOptions(json);
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

        private static IEnumerable<string> ExtractListFromJson(JToken jsonObject, string path)
        {
            return jsonObject
                .SelectTokens(path)
                .Children()
                .OfType<JValue>()
                .Select(jt => jt.Value.ToString());
        }

        private static void FillIgnoreList(ICollection<string> ignoreCollection, JToken jsonObject, string path)
        {
            foreach (string value in ExtractListFromJson(jsonObject, path))
            {
                ignoreCollection.Add(value);
            }
        }

        private void ExtractPlugins(JToken jsonObject)
        {
            var plugins = jsonObject
                .SelectTokens($"..{PluginListConfigNode}")
                .Children()
                .OfType<JProperty>();

            foreach (var plugin in plugins)
            {
                if (TryParsePluginOptions(rootPath, plugin.Value, out var pluginOptions))
                {
                    Plugins.Add(plugin.Name, pluginOptions);
                }
            }
        }

        private bool TryParsePluginOptions(string rootPath, JToken value, out PluginInfo pluginOptions)
        {
            var opt = value
                .Children()
                .OfType<JProperty>();

            pluginOptions = default;

            foreach (var o in opt)
            {
                if (o.Name == "docsBaseUrl")
                {
                    pluginOptions.BaseDocsUrl = o.Value.ToString();
                }
                else if (o.Name == "dll")
                {
                    pluginOptions.AssemblyPath = fileSystem.MakeAbsolutePath(rootPath, o.Value.ToString());
                }
                else if (o.Name == "config")
                {
                    pluginOptions.ConfigPath = fileSystem.MakeAbsolutePath(rootPath, o.Value.ToString());
                }
                else if (o.Name == "docsBasePath")
                {
                    pluginOptions.BaseDocsPath = fileSystem.MakeAbsolutePath(rootPath, o.Value.ToString());
                }
            }

            return !string.IsNullOrEmpty(pluginOptions.AssemblyPath)
                && !string.IsNullOrEmpty(pluginOptions.ConfigPath);
        }

        private void ExtractIgnores(JToken jsonObject)
        {
            FillIgnoreList(IgnoredFolders, jsonObject, ".." + IgnoredFoldersConfigNode);
            FillIgnoreList(IgnoredExtensions, jsonObject, ".." + IgnoredFileExtensionsConfigNode);
        }

        private void ExtractOptions(JToken jsonObject)
        {
            var option = jsonObject
                .SelectTokens(".." + OptionsConfigNode)
                .Children()
                .OfType<JProperty>()
                .FirstOrDefault(opt => opt.Name.Equals("mainBranch", StringComparison.OrdinalIgnoreCase));

            // TODO : shouldn't it do some WriteVerbose?
            if (option != null)
            {
                MainBranch = option.Value.ToString();
            }
        }
    }
}
