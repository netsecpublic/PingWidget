using System;
using System.IO;
using System.Text.Json;
using PingWidget.Models;

namespace PingWidget.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;

        public SettingsService()
        {
            // AppContext.BaseDirectory guarantees the physical path of the .exe, 
            // even when published as a single-file application.
            string exePath = AppContext.BaseDirectory;
            _settingsFilePath = Path.Combine(exePath, "settings.json");
        }

        public ApplicationSettings Load()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<ApplicationSettings>(json);
                    if (settings != null) return settings;
                }
                catch
                {
                    // Fallback to default if file is corrupted
                }
            }
            return new ApplicationSettings();
        }

        public void Save(ApplicationSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
                // Ignore permissions/locking issues
            }
        }
    }
}