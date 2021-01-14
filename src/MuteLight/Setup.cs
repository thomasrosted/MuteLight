using Q42.HueApi;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MuteLight
{
    public static class Setup
    {
        public static async Task<bool> HueConnection(Config config)
        {
            var ips = await HueConnector.GetBridgeIps();

            if (!ips.Any())
            {
                Console.WriteLine(":bridge_at_night: No Hue bridge was found");
                return false;
            }

            var ip = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(":bridge_at_night: Select the ip of the [green]Hue bridge[/] you want to connect to.")
                    .PageSize(5)
                    .AddChoices(ips)
                );

            config.Hue.Ip = ip;
            AnsiConsole.MarkupLine($":bridge_at_night: Added [green]{ip}[/]");
            return true;
        }

        public static async Task HueKey(Config config)
        {
            var client = new LocalHueClient(config.Hue.Ip);
            var machineName = Environment.MachineName;

            AnsiConsole.MarkupLine(":light_bulb: Press the link button on the [green]Hue bridge[/] and then any key to continue.");
            while (true)
            {
                try
                {
                    Console.ReadLine();
                    var appKey = await client.RegisterAsync("MuteLight", machineName);
                    config.Hue.Key = appKey;

                    AnsiConsole.MarkupLine($":light_bulb: Got [green]{appKey}[/] key from application-device [green]MuteLight-{machineName}[/].");
                    return;
                }
                catch (LinkButtonNotPressedException)
                {
                    AnsiConsole.MarkupLine(":light_bulb: The link button on the [green]Hue bridge[/] must be pressed.");
                }
            }
        }

        public static async Task Lights(Config config, HueConnector hueConnector)
        {
            var lights = await hueConnector.GetLights();
            var selectedlights = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select the [green]lights[/] that you wish to be updated when mute state changes.")
                    .Required()
                    .PageSize(10)
                    .AddChoices(lights.Select(x => $"{x.Id}: {x.Name} in {x.GroupString}")));

            config.Hue.Lights = selectedlights.Select(x => x.Split(':').First()).ToList();
        }
    }
}