using Q42.HueApi;
using Q42.HueApi.ColorConverters;
using Q42.HueApi.ColorConverters.HSB;
using Q42.HueApi.Models.Groups;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MuteLight
{
    public class HueConnector
    {
        private readonly LocalHueClient _hueClient;

        public HueConnector(Config config)
        {
            _hueClient = new LocalHueClient(config.Hue.Ip);
            _hueClient.Initialize(config.Hue.Key);
        }

        public void SetColor(RGBColor color, List<string> lights)
        {
            var command = new LightCommand();
            command.TurnOn().SetColor(color);

            _hueClient.SendCommandAsync(command, lights);
        }

        public static async Task<List<string>> GetBridgeIps()
        {
            var bridges = await new HttpBridgeLocator().LocateBridgesAsync(TimeSpan.FromSeconds(15));

            return bridges.Select(locatedBridge => locatedBridge.IpAddress).ToList();
        }

        public async Task<List<Light>> GetLights()
        {
            var lights = (await _hueClient.GetLightsAsync()).ToDictionary(x => x.Id, x => new Light { Id = x.Id, Name = x.Name });

            var lightsWithGroups = new List<Light>();

            foreach (var group in (await _hueClient.GetGroupsAsync()).OrderBy(x => x.Type != GroupType.Room))
            {
                foreach (var groupLight in group.Lights)
                {
                    var existingItem = lightsWithGroups.FirstOrDefault(x => x.Id == groupLight);
                    if (existingItem != null)
                    {
                        existingItem.Groups.Add(group.Name);
                    }
                    else
                    {
                        var newItem = lights[groupLight];
                        newItem.Groups.Add(group.Name);
                        lightsWithGroups.Add(newItem);
                    }
                }
            }

            return lightsWithGroups.OrderBy(x => int.Parse(x.Id)).ToList();
        }

        public class Light
        {
            public string Id { get; set; }

            public List<string> Groups { get; set; } = new List<string>();

            public string GroupString => string.Join(", ", Groups);

            public string Name { get; set; }
        }
    }
}