using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace PingWidget.Services
{
    public class TrayIconService : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly DispatcherTimer _flashTimer;
        private Icon? _defaultIcon;
        private Icon? _blankIcon;
        private bool _isIconVisible = true;

        public event EventHandler? OpenClicked;
        public event EventHandler? PauseClicked;
        public event EventHandler? SettingsClicked;
        public event EventHandler? ExitClicked;

        public TrayIconService()
        {
            // 1. Generate a transparent/blank icon in memory for the flashing effect
            using (var bmp = new Bitmap(16, 16))
            {
                _blankIcon = Icon.FromHandle(bmp.GetHicon());
            }

            // 2. Load your custom ping2.ico from the application resources
            try
            {
                var streamInfo = Application.GetResourceStream(new Uri("pack://application:,,,/ping2.ico"));
                if (streamInfo != null)
                {
                    _defaultIcon = new Icon(streamInfo.Stream);
                }
            }
            catch
            {
                // Fallback if ping2.ico isn't found
                _defaultIcon = SystemIcons.Information;
            }

            // 3. Initialize the tray icon
            _notifyIcon = new NotifyIcon
            {
                Icon = _defaultIcon,
                Visible = true,
                Text = "PingWidget"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open / Restore", null, (s, e) => OpenClicked?.Invoke(this, EventArgs.Empty));
            contextMenu.Items.Add("Pause / Resume Monitoring", null, (s, e) => PauseClicked?.Invoke(this, EventArgs.Empty));
            contextMenu.Items.Add("Settings", null, (s, e) => SettingsClicked?.Invoke(this, EventArgs.Empty));
            contextMenu.Items.Add("-");
            contextMenu.Items.Add("Exit", null, (s, e) => ExitClicked?.Invoke(this, EventArgs.Empty));

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += (s, e) => OpenClicked?.Invoke(this, EventArgs.Empty);

            // 4. Setup the flashing timer (swaps icon every 500 milliseconds)
            _flashTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _flashTimer.Tick += FlashTimer_Tick;
        }

        private void FlashTimer_Tick(object? sender, EventArgs e)
        {
            _isIconVisible = !_isIconVisible;
            _notifyIcon.Icon = _isIconVisible ? _defaultIcon : _blankIcon;
        }

        public void StartFlashing()
        {
            if (!_flashTimer.IsEnabled)
            {
                _isIconVisible = true;
                _flashTimer.Start();
            }
        }

        public void StopFlashing()
        {
            _flashTimer.Stop();
            _isIconVisible = true;
            _notifyIcon.Icon = _defaultIcon;
        }

        public void ShowBalloon(string title, string message)
        {
            _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Warning);
        }

        public void Dispose()
        {
            _flashTimer.Stop();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _blankIcon?.Dispose();
            _defaultIcon?.Dispose();
        }
    }
}