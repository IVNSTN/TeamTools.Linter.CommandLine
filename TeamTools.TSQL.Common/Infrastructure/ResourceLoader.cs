using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;

namespace TeamTools.Common.Linting.Infrastructure
{
    public class ResourceLoader<T> : BaseJsonConfigLoader<T>
        where T : BaseLintingConfig
    {
        public ResourceLoader(T config)
        {
            this.Config = config;
        }

        protected T Config { get; }

        protected override void FillConfig(T config, JToken json)
        {
            LoadRuleTranslation(config, json);
        }

        protected override T MakeConfig()
        {
            // not making anything new
            return Config;
        }

        // TODO : use deserialize to dictionary
        private static void LoadRuleTranslation(T config, JToken json)
        {
            Debug.WriteLine("LoadRuleTranslation");
            Debug.Assert(config != null, "config not set");
            Debug.Assert(json != null, "json not set");

            var rulesTranslations = json.SelectTokens("..messages")
                .Children()
                .OfType<JProperty>();

            foreach (var configValue in rulesTranslations)
            {
                var ruleId = configValue.Name;
                var ruleMessage = configValue.Value.ToString();

                if (config.Rules.ContainsKey(ruleId))
                {
                    config.Rules[ruleId] = ruleMessage;
                }
            }
        }
    }
}
