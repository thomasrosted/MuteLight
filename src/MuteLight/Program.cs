using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi.Observables;
using AudioSwitcher.AudioApi.Session;
using Spectre.Console;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MuteLight
{
    public static class Program
    {
        private static IDevice _device;

        static async Task Main(string[] args)
        {
            var (config, hueConnector) = await SetupConfig(args);

            var controller = new CoreAudioController();
            controller.AudioDeviceChanged.Subscribe(x =>
            {
                if (x.ChangedType == DeviceChangedType.DefaultChanged && (_device == null || _device.Id != controller.DefaultCaptureCommunicationsDevice.Id))
                {
                    _device = controller.DefaultCaptureCommunicationsDevice;
                    ObserveDevice(_device, hueConnector, config);
                };
            });

            _device = controller.DefaultCaptureCommunicationsDevice;

            if (_device != null)
            {
                ObserveDevice(_device, hueConnector, config);
            }

            Console.ReadKey(true);
        }

        private static async Task<(Config config, HueConnector hueConnector)> SetupConfig(string[] args)
        {
            if (args.Length > 0 && args.First() == "--reset")
            {
                Config.ResetConfig();
            }

            Config.EnsureConfig();
            var config = Config.ReadConfig();

            if (string.IsNullOrEmpty(config.Hue.Ip))
            {
                if (!await Setup.HueConnection(config))
                {
                    Environment.Exit(1);
                }

                Config.WriteConfig(config);
            }

            if (string.IsNullOrEmpty(config.Hue.Key))
            {
                await Setup.HueKey(config);
                Config.WriteConfig(config);
            }

            var hueConnector = new HueConnector(config);

            if (!config.Hue.Lights.Any())
            {
                await Setup.Lights(config, hueConnector);
                Config.WriteConfig(config);
            }

            return (config, hueConnector);
        }

        private static void ObserveDevice(IDevice device, HueConnector hueConnector, Config config)
        {
            AnsiConsole.MarkupLine($":microphone: [grey]Device:[/] [lightslategrey]{device.Name}[/]");
            var muteObserver = new MuteStateObserver(hueConnector, config);

            var sessionController = device.GetCapability<IAudioSessionController>();

            foreach (var session in sessionController.All())
            {
                if (session.ProcessId == 0) continue;

                muteObserver.SubscribeToSession(session);
            }

            sessionController.SessionCreated.Subscribe(muteObserver);
            sessionController.SessionDisconnected.Subscribe(muteObserver);
        }
    }
}
