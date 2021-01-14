using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.IO;

namespace MuteLight
{
    public class Config
    {
        private static readonly string _config = @".\config.json";
        private static readonly string _defaultConfig = @".\config.default.json";

        private Config() { }

        public Colors Colors { get; set; } = new Colors();

        public Hue Hue { get; set; } = new Hue();

        public static void EnsureConfig()
        {
            if (!File.Exists(_config))
            {
                ResetConfig();
            }
        }

        public static void ResetConfig()
        {
            File.Copy(_defaultConfig, _config, true);
        }

        public static Config ReadConfig()
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(_config));
        }

        public static void WriteConfig(Config config)
        {
            File.WriteAllText(@".\config.json", JsonConvert.SerializeObject(config, Formatting.Indented, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            }));
        }
    }

    public class Hue
    {
        public string Ip { get; set; }

        public string Key { get; set; }

        public List<string> Lights { get; set; } = new List<string>();
    }

    public class Colors
    {
        public string Muted { get; set; }
        public string Unmuted { get; set; }
        public string NotInSession { get; set; }
    }

}