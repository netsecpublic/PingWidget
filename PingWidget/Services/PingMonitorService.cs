using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace PingWidget.Services
{
    public class PingResult
    {
        public bool Success { get; set; }
        public long RoundtripTime { get; set; }
    }

    public class PingMonitorService
    {
        public async Task<PingResult> SendPingAsync(string hostname, int timeoutMs = 2000, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(hostname))
            {
                return new PingResult { Success = false, RoundtripTime = 0 };
            }

            try
            {
                using var ping = new Ping();
                byte[] buffer = new byte[32];
                var options = new PingOptions { DontFragment = true };

                var reply = await ping.SendPingAsync(hostname.Trim(), timeoutMs, buffer, options).ConfigureAwait(false);

                if (reply.Status == IPStatus.Success)
                {
                    return new PingResult
                    {
                        Success = true,
                        RoundtripTime = Math.Max(reply.RoundtripTime, 1)
                    };
                }
            }
            catch
            {
                // Handle DNS failures, invalid hostnames, unreachable networks
            }

            return new PingResult { Success = false, RoundtripTime = 0 };
        }
    }
}