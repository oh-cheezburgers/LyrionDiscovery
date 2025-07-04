using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace LmsDiscovery
{
    /// <summary>
    /// Discovers Logitech Media Servers on the local network using UDP broadcast.
    /// </summary>
    /// <summary>
    /// Provides methods to discover Logitech Media Servers on the local network.
    /// </summary>
    public static class Discoverer
    {
        /// <summary>
        /// Discovers Logitech Media Servers within the specified timeout using the provided UdpClient.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="requestTimeout">The maximum time to wait for server responses.</param>
        /// <param name="udpClient">The UdpClient instance used for sending and receiving UDP packets.</param>
        /// <param name="port">The UDP port to use for discovery (default is 3483).</param>
        /// <returns>A list of server response strings received during discovery.</returns>
        public static IReadOnlyList<string> Discover(CancellationToken cancellationToken, TimeSpan requestTimeout, UdpClient udpClient, int port = 3483)
        {
            ArgumentNullException.ThrowIfNull(udpClient);

            using (udpClient)
            {
                var servers = new List<string>();
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                udpClient.EnableBroadcast = true;
                udpClient.Client.ReceiveTimeout = (int)requestTimeout.TotalMilliseconds;

                var data = Encoding.UTF8.GetBytes("eIPAD\0NAME\0VERS\0UUID\0JSON\0CLIP\0");
                udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));

                var from = new IPEndPoint(0, 0);
                while (true)
                {
                    try
                    {
                        var recvBuffer = udpClient.Receive(ref from);
                        var response = Encoding.UTF8.GetString(recvBuffer);
                        servers.Add(response);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                }

                return servers;
            }
        }

        /// <summary>
        /// Maps a dictionary of key-value pairs to a <see cref="MediaServer"/> object.
        /// </summary>
        /// <returns>A <see cref="MediaServer"/> object populated with data extracted from the response string.</returns>
        public static MediaServer Map(IDictionary<string, string> keyValuePairs)
        {
            var server = new MediaServer
            {
                Name = keyValuePairs.TryGetValue("NAME", out var name) ? name : null,
                Version = keyValuePairs.TryGetValue("VERS", out var version) ? version : null,
                UUID = keyValuePairs.TryGetValue("UUID", out var uuid) ? uuid : null,
                Json = keyValuePairs.TryGetValue("JSON", out var json) ? json : null,
                Clip = keyValuePairs.TryGetValue("CLIP", out var clip) ? clip : null
            };

            return server;
        }

        /// <summary>
        /// Parses a server response string into a dictionary of key-value pairs.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Parse(string response)
        {
            var regex = new Regex(@"(NAME|VERS|JSON|CLIP)[\p{Cc}]|(UUID)[\$]");
            var delimiters = new[] { "NAME", "VERS", "UUID", "JSON", "CLIP" };
            var parts = regex.Split(response);

            var dict = new Dictionary<string, string>();
            string? lastKey = null;
            foreach (var part in parts)
            {
                if (part == "E")
                {
                    continue; // Skip the initial 'E' character
                }
                if (delimiters.Contains(part))
                {
                    lastKey = part;
                    continue;
                }
                if (lastKey != null)
                {
                    dict.Add(lastKey, part);
                    lastKey = null;
                }
            }

            return dict;
        }
    }
}
