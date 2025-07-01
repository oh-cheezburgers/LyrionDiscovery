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
    public class Discoverer
    {
        /// <summary>
        /// Discovers Logitech Media Servers within the specified timeout using the provided UdpClient.
        /// </summary>
        /// <param name="timeout">The maximum time to wait for server responses.</param>
        /// <param name="udpClient">The UdpClient instance used for sending and receiving UDP packets.</param>
        /// <returns>A list of server response strings received during discovery.</returns>
        public List<string> Discover(TimeSpan timeout, UdpClient udpClient)
        {
            var servers = new List<string>();

            int PORT = 3483;
            udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));
            udpClient.EnableBroadcast = true;
            udpClient.Client.ReceiveTimeout = (int)timeout.TotalMilliseconds;

            var data = Encoding.UTF8.GetBytes("eIPAD\0NAME\0VERS\0UUID\0JSON\0CLIP\0");
            udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, PORT));

            var from = new IPEndPoint(0, 0);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed < timeout)
            {
                try
                {
                    var recvBuffer = udpClient.Receive(ref from);
                    var response = Encoding.UTF8.GetString(recvBuffer);
                    servers.Add(response);
                }
                catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
                {
                    break;
                }
            }

            return servers;
        }

        /// <summary>
        /// Maps a server response string to a Server object.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>A <see cref="Server"/> object populated with data extracted from the response string.</returns>
        public Server Map(string response)
        {
            var server = new Server
            {
                Name = new Regex("(?<=NAME).*(?=VERS)").Match(response).Value,
                Version = new Regex("(?<=VERS).*(?=UUID)").Match(response).Value,
                UUID = new Regex("(?<=UUID\\$).*(?=JSON)").Match(response).Value,
                Json = new Regex("(?<=JSON).*(?=CLIP)").Match(response).Value,
                Clip = new Regex("(?<=CLIP).*").Match(response).Value,
            };

            return server;
        }
    }
}
