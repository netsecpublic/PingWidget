using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PingWidget.Models;
using PingWidget.Services;

namespace PingWidget.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsService _settingsService;
        private ApplicationSettings _settings;

        public ObservableCollection<ServerConfiguration> Servers { get; } = new();

        private ServerConfiguration? _selectedServer;
        public ServerConfiguration? SelectedServer
        {
            get => _selectedServer;
            set { _selectedServer = value; OnPropertyChanged(); }
        }

        public double TransparencyPercentage
        {
            get => _settings.Transparency * 100.0;
            set
            {
                _settings.Transparency = Math.Clamp(value / 100.0, 0.2, 1.0);
                OnPropertyChanged();

                if (System.Windows.Application.Current.MainWindow?.DataContext is MainViewModel mainVm)
                {
                    mainVm.Transparency = _settings.Transparency;
                }
            }
        }

        public bool AlwaysOnTop
        {
            get => _settings.AlwaysOnTop;
            set { _settings.AlwaysOnTop = value; OnPropertyChanged(); }
        }

        public int PingIntervalSeconds
        {
            get => _settings.PingIntervalSeconds;
            set { _settings.PingIntervalSeconds = value; OnPropertyChanged(); }
        }

        public bool UseDefaultSound
        {
            get => _settings.UseDefaultSound;
            set { _settings.UseDefaultSound = value; OnPropertyChanged(); }
        }

        public string? CustomWaveFilePath
        {
            get => _settings.CustomWaveFilePath;
            set { _settings.CustomWaveFilePath = value; OnPropertyChanged(); }
        }

        public bool StartWithWindows
        {
            get => _settings.StartWithWindows;
            set { _settings.StartWithWindows = value; OnPropertyChanged(); }
        }

        public ICommand AddServerCommand { get; }
        public ICommand RemoveServerCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand TestSoundCommand { get; }

        public event Action? RequestClose;

        public SettingsViewModel()
        {
            _settingsService = new SettingsService();
            _settings = _settingsService.Load();

            foreach (var s in _settings.Servers)
            {
                Servers.Add(s);
            }

            AddServerCommand = new RelayCommand(_ => AddServer());
            // We removed the strict 'CanExecute' rule so WPF stops turning it white
            RemoveServerCommand = new RelayCommand(param => RemoveServer(param));
            MoveUpCommand = new RelayCommand(_ => MoveUp());
            MoveDownCommand = new RelayCommand(_ => MoveDown());
            SaveCommand = new RelayCommand(_ => SaveAndClose());
            TestSoundCommand = new RelayCommand(_ => TestSound());
        }

        private void AddServer()
        {
            var newServer = new ServerConfiguration { Hostname = "new.server.com", LatencyThresholdMs = 100 };
            Servers.Add(newServer);
            SelectedServer = newServer;
        }

        private void RemoveServer(object? param)
        {
            if (SelectedServer != null)
            {
                Servers.Remove(SelectedServer);
                SelectedServer = null;
            }
        }

        private void MoveUp()
        {
            if (SelectedServer == null) return;
            int index = Servers.IndexOf(SelectedServer);
            if (index > 0)
            {
                var item = SelectedServer;
                Servers.RemoveAt(index);
                Servers.Insert(index - 1, item);
                SelectedServer = item;
            }
        }

        private void MoveDown()
        {
            if (SelectedServer == null) return;
            int index = Servers.IndexOf(SelectedServer);
            if (index >= 0 && index < Servers.Count - 1)
            {
                var item = SelectedServer;
                Servers.RemoveAt(index);
                Servers.Insert(index + 1, item);
                SelectedServer = item;
            }
        }

        private void TestSound()
        {
            var audio = new AudioService();
            _ = audio.PlayAlertAsync(UseDefaultSound, CustomWaveFilePath);
        }

        private void SaveAndClose()
        {
            _settings.Servers.Clear();
            foreach (var s in Servers)
            {
                _settings.Servers.Add(s);
            }
            _settingsService.Save(_settings);
            RequestClose?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}