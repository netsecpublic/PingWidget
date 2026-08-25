using System;

namespace PingWidget.Models
{
    public class ServerConfiguration
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Hostname { get; set; } = string.Empty;
        public int LatencyThresholdMs { get; set; } = 100;
        public bool IsEnabled { get; set; } = true;
        public bool AlarmEnabled { get; set; } = true;
        public string? CustomSoundPath { get; set; }
    }
}