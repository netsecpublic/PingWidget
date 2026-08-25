using System;
using System.Windows;
using System.Windows.Input;
using PingWidget.ViewModels;

namespace PingWidget.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            _viewModel.RequestRestoreFromTray += RestoreFromTray;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e) => OpenSettings();
        private void Minimize_Click(object sender, RoutedEventArgs e) => MinimizeToTray();
        private void Exit_Click(object sender, RoutedEventArgs e) => CloseApp();

        private void MinimizeToTray()
        {
            Hide();
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            // NOTE: Activate() is intentionally omitted so it pops up visually 
            // without stealing keyboard focus from whatever window you are typing in!
            Topmost = true;
            Topmost = _viewModel.AlwaysOnTop;
        }

        private void OpenSettings()
        {
            var settingsWin = new SettingsWindow();
            settingsWin.Owner = this;
            if (settingsWin.ShowDialog() == true)
            {
                _viewModel.ReloadSettings();
            }
        }

        private void CloseApp()
        {
            _viewModel.Dispose();
            Application.Current.Shutdown();
        }
    }
}