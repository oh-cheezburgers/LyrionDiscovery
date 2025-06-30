using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LmsDiscovery
{
    public class Discoverer
    {
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
    }
}
