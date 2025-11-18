using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;

namespace TeamTools.Common.Linting
{
    public abstract class BaseJsonConfigLoader<T>
    {
        private static readonly JsonLoadSettings LoadSettings = new JsonLoadSettings
        {
            LineInfoHandling = LineInfoHandling.Ignore,
            CommentHandling = CommentHandling.Ignore,
        };

        public static bool TryParseJson(string jsonString, out JToken token)
        {
            try
            {
                token = JToken.Parse(jsonString, LoadSettings);
                return true;
            }
            catch
            {
                token = null;
                return false;
            }
        }

        public T LoadConfig(string configPath)
        {
            Debug.WriteLine("LoadConfig 1 " + configPath);
            if (!File.Exists(configPath))
            {
                // TODO : or throw new FileNotFoundException("Config not found", configPath); ???
                return default;
            }

            using (var reader = File.OpenText(configPath))
            {
                return LoadConfig(reader);
            }
        }

        public T LoadConfig(TextReader configSource)
        {
            Debug.WriteLine("LoadConfig 2 " + configSource?.ToString());
            if (!TryParseJson(configSource?.ReadToEnd(), out var json) || json is null)
            {
                return default;
            }

            var config = MakeConfig();
            FillConfig(config, json);

            return config;
        }

        protected abstract T MakeConfig();

        protected abstract void FillConfig(T config, JToken src);
    }
}
