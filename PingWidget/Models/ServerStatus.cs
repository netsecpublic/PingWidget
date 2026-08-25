using System;

namespace PingWidget.Models
{
    public enum ServerState
    {
        Green,
        Orange,
        Red,
        Offline
    }

    public enum MuteDurationType
    {
        None,
        Minutes5,
        Minutes15,
        Minutes30,
        Hour1,
        Hours4,
        UntilTomorrow,
        Forever
    }

    public class ServerStatus
    {
        public string ServerId { get; set; } = string.Empty;
        public ServerState State { get; set; } = ServerState.Green;
        public long? ResponseTimeMs { get; set; }
        public int ConsecutiveFailures { get; set; }
        public int ConsecutiveHighLatency { get; set; }
        public bool IsMuted { get; set; }
        public DateTime? MuteExpiration { get; set; }
        public bool AlarmTriggered { get; set; }
    }
}