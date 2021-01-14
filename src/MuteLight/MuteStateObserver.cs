using AudioSwitcher.AudioApi.Session;
using Q42.HueApi.ColorConverters;
using Spectre.Console;
using System;
using System.Collections.Generic;

namespace MuteLight
{
    public class MuteStateObserver : IObserver<SessionMuteChangedArgs>, IObserver<IAudioSession>, IObserver<SessionStateChangedArgs>, IObserver<string>
    {
        private readonly HueConnector _hueConnector;
        private readonly List<string> _lights;
        private readonly RGBColor _muted;
        private readonly RGBColor _unmuted;
        private readonly RGBColor _notInSession;

        public MuteStateObserver(HueConnector hueConnector, Config config)
        {
            _hueConnector = hueConnector;
            _lights = config.Hue.Lights;
            _muted = new RGBColor(config.Colors.Muted);
            _unmuted = new RGBColor(config.Colors.Unmuted);
            _notInSession = new RGBColor(config.Colors.NotInSession);
        }

        private void SetMuteState(bool isMuted)
        {
            AnsiConsole.MarkupLine($"{(isMuted ? ":speak_no_evil_monkey:" : ":monkey_face:")} [grey]Microphone:[/] [lightslategrey]{(isMuted ? "Muted" : "Unmuted")}[/]");
            _hueConnector.SetColor(isMuted ? _muted : _unmuted, _lights);
        }

        public void OnNext(SessionMuteChangedArgs value)
        {
            if (value.Session.SessionState == AudioSessionState.Active)
            {
                var isMuted = value.Session.IsMuted;

                SetMuteState(isMuted);
            }
        }

        public void OnNext(IAudioSession value)
        {
            SubscribeToSession(value);
        }

        public void SubscribeToSession(IAudioSession value)
        {
            AnsiConsole.MarkupLine($":handshake: [grey]Session ([/][grey69]{GetProcessName(value)}[/][grey]):[/] [lightslategrey]Subscribed[/]");
            value.MuteChanged.Subscribe(this);
            value.StateChanged.Subscribe(this);
        }

        private static string GetProcessName(IAudioSession value)
        {
            return Utils.GetProcessName(value.ProcessId);
        }

        public void OnNext(SessionStateChangedArgs value)
        {
            if (value.State == AudioSessionState.Active)
            {
                AnsiConsole.MarkupLine($":call_me_hand: [grey]Session ([/][grey69]{GetProcessName(value.Session)}[/][grey]):[/] [lightslategrey]Started[/]");
                SetMuteState(value.Session.IsMuted);
            }
            else
            {
                AnsiConsole.MarkupLine($":waving_hand: [grey]Session ([/][grey69]{GetProcessName(value.Session)}[/][grey]):[/] [lightslategrey]Ended[/]");
                _hueConnector.SetColor(_notInSession, _lights);
            }
        }

        public void OnNext(string value)
        {
            _hueConnector.SetColor(_notInSession, _lights);
        }

        public void OnError(Exception error)
        {
            Console.WriteLine(error);
        }

        public void OnCompleted()
        {
        }
    }
}