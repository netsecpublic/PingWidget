using System;
using System.IO;
using System.Media;
using System.Threading.Tasks;

namespace PingWidget.Services
{
    public class AudioService
    {
        public async Task PlayAlertAsync(bool useDefault, string? customPath)
        {
            await Task.Run(() =>
            {
                try
                {
                    if (!useDefault && !string.IsNullOrWhiteSpace(customPath) && File.Exists(customPath))
                    {
                        using var player = new SoundPlayer(customPath);
                        player.PlaySync();
                        return;
                    }
                }
                catch
                {
                    // Fallback to default system sound if custom fails
                }

                try
                {
                    System.Media.SystemSounds.Exclamation.Play();
                }
                catch
                {
                    Console.Beep(800, 300);
                }
            });
        }
    }
}