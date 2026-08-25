using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using PingWidget.Models;

namespace PingWidget.ViewModels
{
    public class ServerViewModel : INotifyPropertyChanged
    {
        private ServerConfiguration _config;
        private ServerStatus _status;
        private Brush _backgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A2A"));
        private Brush _valueColor = Brushes.Gray;
        private string _latencyLabel = "Ping:";
        private string _latencyValue = "...";
        private bool _isMuted;
        private DateTime? _muteExpiration;

        public ICommand MuteCommand { get; }
        public ICommand UnmuteCommand { get; }
        public ICommand OpenCmdCommand { get; }

        public ServerViewModel(ServerConfiguration config)
        {
            _config = config;
            _status = new ServerStatus { ServerId = config.Id };

            MuteCommand = new RelayCommand(param => Mute(param?.ToString()));
            UnmuteCommand = new RelayCommand(_ => Unmute());
            OpenCmdCommand = new RelayCommand(_ => OpenCmd());

            UpdateDisplay();
        }

        public ServerConfiguration Configuration
        {
            get => _config;
            set { _config = value; OnPropertyChanged(); OnPropertyChanged(nameof(Hostname)); }
        }

        public string Hostname => _config.Hostname;

        public ServerStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                if (_status != null)
                {
                    _isMuted = _status.IsMuted;
                    _muteExpiration = _status.MuteExpiration;
                }
                UpdateDisplay();
            }
        }

        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            set { _backgroundBrush = value; OnPropertyChanged(); }
        }

        public Brush ValueColor
        {
            get => _valueColor;
            set { _valueColor = value; OnPropertyChanged(); }
        }

        public string LatencyLabel
        {
            get => _latencyLabel;
            set { _latencyLabel = value; OnPropertyChanged(); }
        }

        public string LatencyValue
        {
            get => _latencyValue;
            set { _latencyValue = value; OnPropertyChanged(); }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                if (_isMuted != value)
                {
                    _isMuted = value;
                    if (_status != null) _status.IsMuted = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MuteIndicator));
                }
            }
        }

        public string MuteIndicator => _isMuted ? "🔇" : string.Empty;

        private void Mute(string? durationStr)
        {
            TimeSpan? duration = durationStr switch
            {
                "1" => TimeSpan.FromMinutes(1),
                "10" => TimeSpan.FromMinutes(10),
                "30" => TimeSpan.FromMinutes(30),
                "60" => TimeSpan.FromHours(1),
                "180" => TimeSpan.FromHours(3),
                "300" => TimeSpan.FromHours(5),
                "1440" => TimeSpan.FromHours(24),
                "forever" => null,
                _ => TimeSpan.FromMinutes(30)
            };

            IsMuted = true;
            _muteExpiration = duration.HasValue ? DateTime.Now.Add(duration.Value) : null;
            if (_status != null)
            {
                _status.IsMuted = true;
                _status.MuteExpiration = _muteExpiration;
            }
            UpdateDisplay();
        }

        private void Unmute()
        {
            IsMuted = false;
            _muteExpiration = null;
            if (_status != null)
            {
                _status.IsMuted = false;
                _status.MuteExpiration = null;
                _status.AlarmTriggered = false;
            }
            UpdateDisplay();
        }

        private void OpenCmd()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = true
                });
            }
            catch
            {
                // Fallback
            }
        }

        public void UpdateDisplay()
        {
            if (_status != null)
            {
                if (_status.IsMuted && _status.MuteExpiration.HasValue)
                {
                    if (DateTime.Now >= _status.MuteExpiration.Value)
                    {
                        _status.IsMuted = false;
                        _status.MuteExpiration = null;
                        IsMuted = false;
                    }
                }
                else if (!_status.IsMuted && _isMuted)
                {
                    IsMuted = false;
                }
            }

            LatencyValue = _status?.ResponseTimeMs.HasValue == true ? $"{_status.ResponseTimeMs} ms" : "TIMEOUT";

            if (!_config.IsEnabled)
            {
                BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A"));
                ValueColor = Brushes.DimGray;
                LatencyLabel = "Status:";
                LatencyValue = "DISABLED";
                return;
            }

            LatencyLabel = "Ping:";

            switch (_status?.State)
            {
                case ServerState.Green:
                    BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#182b1a"));
                    ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#66cc66"));
                    break;
                case ServerState.Orange:
                    BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#332411"));
                    ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ffaa00"));
                    break;
                case ServerState.Red:
                case ServerState.Offline:
                    BackgroundBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#331111"));
                    ValueColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff4444"));
                    LatencyValue = "OFFLINE";
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}