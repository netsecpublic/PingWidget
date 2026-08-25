using System.Collections.Generic;

namespace PingWidget.Models
{
    public class ApplicationSettings
    {
        public List<ServerConfiguration> Servers { get; set; } = new();
        public double Transparency { get; set; } = 1.0;
        public bool AlwaysOnTop { get; set; } = true;
        public int PingIntervalSeconds { get; set; } = 3;
        public bool UseDefaultSound { get; set; } = true;
        public string? CustomWaveFilePath { get; set; }
        public bool StartWithWindows { get; set; } = false;
    }
}