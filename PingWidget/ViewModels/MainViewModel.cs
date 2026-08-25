using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PingWidget.Models;
using PingWidget.Services;

namespace PingWidget.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SettingsService _settingsService;
        private readonly PingMonitorService _pingService;
        private readonly AudioService _audioService;
        private readonly TrayIconService _trayService;
        private ApplicationSettings _settings;
        private CancellationTokenSource? _cts;

        private double _transparency = 1.0;
        private bool _alwaysOnTop = true;
        private bool _isMonitoring = true;
        private bool _isNoServersVisible = true;

        public ObservableCollection<ServerViewModel> Servers { get; } = new();

        public double Transparency
        {
            get => _transparency;
            set
            {
                if (_transparency != value)
                {
                    _transparency = value;
                    OnPropertyChanged();
                    _settings.Transparency = value;
                    Save();
                }
            }
        }

        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set { _alwaysOnTop = value; OnPropertyChanged(); _settings.AlwaysOnTop = value; Save(); }
        }

        public bool IsNoServersVisible
        {
            get => _isNoServersVisible;
            set { _isNoServersVisible = value; OnPropertyChanged(); }
        }

        public event Action? RequestOpenSettings;
        public event Action? RequestRestoreFromTray;
        public event Action? RequestExit;

        public MainViewModel()
        {
            _settingsService = new SettingsService();
            _pingService = new PingMonitorService();
            _audioService = new AudioService();
            _trayService = new TrayIconService();

            _settings = _settingsService.Load();
            ApplySettingsToProperties();

            _trayService.OpenClicked += (s, e) => RequestRestoreFromTray?.Invoke();
            _trayService.PauseClicked += (s, e) => _isMonitoring = !_isMonitoring;
            _trayService.SettingsClicked += (s, e) => RequestOpenSettings?.Invoke();
            _trayService.ExitClicked += (s, e) => RequestExit?.Invoke();

            StartMonitoring();
        }

        private void ApplySettingsToProperties()
        {
            _transparency = _settings.Transparency;
            _alwaysOnTop = _settings.AlwaysOnTop;

            OnPropertyChanged(nameof(Transparency));
            OnPropertyChanged(nameof(AlwaysOnTop));

            Servers.Clear();
            foreach (var cfg in _settings.Servers)
            {
                Servers.Add(new ServerViewModel(cfg));
            }
            IsNoServersVisible = Servers.Count == 0;
        }

        public void ReloadSettings()
        {
            _settings = _settingsService.Load();
            ApplySettingsToProperties();
        }

        private void StartMonitoring()
        {
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => MonitorLoopAsync(_cts.Token));
        }

        private async Task MonitorLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_isMonitoring)
                {
                    foreach (var serverVm in Servers)
                    {
                        if (!serverVm.Configuration.IsEnabled) continue;

                        var result = await _pingService.SendPingAsync(serverVm.Configuration.Hostname, 2000, token);

                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            UpdateServerStatus(serverVm, result);
                        });
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_settings.PingIntervalSeconds), token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private void UpdateServerStatus(ServerViewModel svm, PingResult result)
        {
            var st = svm.Status;
            var cfg = svm.Configuration;
            var previousState = st.State;

            if (svm.IsMuted != st.IsMuted)
            {
                st.IsMuted = svm.IsMuted;
            }

            if (st.IsMuted && st.MuteExpiration.HasValue && DateTime.Now >= st.MuteExpiration.Value)
            {
                st.IsMuted = false;
                st.MuteExpiration = null;
                st.AlarmTriggered = false;
                svm.IsMuted = false;
            }
            else if (st.IsMuted && !svm.IsMuted)
            {
                svm.IsMuted = true;
            }

            if (result.Success)
            {
                st.ResponseTimeMs = result.RoundtripTime;
                st.ConsecutiveFailures = 0;

                if (result.RoundtripTime > cfg.LatencyThresholdMs)
                {
                    st.ConsecutiveHighLatency++;
                    if (st.ConsecutiveHighLatency >= 3)
                    {
                        st.State = ServerState.Orange;
                    }
                }
                else
                {
                    st.ConsecutiveHighLatency = 0;
                    st.State = ServerState.Green;
                }
            }
            else
            {
                st.ResponseTimeMs = null;
                st.ConsecutiveHighLatency = 0;
                st.ConsecutiveFailures++;

                if (st.ConsecutiveFailures >= 3)
                {
                    st.State = ServerState.Red;
                }
            }

            // Process Alarms & Audio
            if (svm.IsMuted || st.IsMuted)
            {
                st.AlarmTriggered = false;
            }
            else
            {
                if (st.State == ServerState.Green)
                {
                    st.AlarmTriggered = false;
                }
                else if (st.State == ServerState.Orange && previousState == ServerState.Green)
                {
                    TriggerAlarm(cfg, st, "High Latency", $"Server {cfg.Hostname} is experiencing high latency ({st.ResponseTimeMs} ms).");
                }
                else if (st.State == ServerState.Red && previousState != ServerState.Red)
                {
                    TriggerAlarm(cfg, st, "Server Outage", $"Server {cfg.Hostname} has failed 3 consecutive pings!");
                }
            }

            // GLOBAL TRAY FLASHING LOGIC
            // Ensure the tray only flashes if at least ONE server is actively triggering an alarm and is unmuted.
            if (Servers.Any(s => s.Status.AlarmTriggered))
            {
                _trayService.StartFlashing();
            }
            else
            {
                _trayService.StopFlashing();
            }

            svm.UpdateDisplay();
        }

        private void TriggerAlarm(ServerConfiguration cfg, ServerStatus st, string title, string message)
        {
            if (cfg.AlarmEnabled)
            {
                st.AlarmTriggered = true;
                _trayService.ShowBalloon(title, message);
                _ = _audioService.PlayAlertAsync(_settings.UseDefaultSound, _settings.CustomWaveFilePath);
                RequestRestoreFromTray?.Invoke();
            }
        }

        private void Save()
        {
            _settingsService.Save(_settings);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _trayService.Dispose();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}