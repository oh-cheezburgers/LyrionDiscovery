using LmsDiscovery.UdpClient;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LmsDiscovery
{
    /// <summary>
    /// Discovers Logitech Media Servers on the local network using UDP broadcast.
    /// </summary>
    /// <summary>
    /// Provides methods to discover Logitech Media Servers on the local network.
    /// </summary>
    public static class Discovery
    {
        /// <summary>
        /// Discovers Logitech Media Servers within the specified timeout using the provided UdpClient.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <param name="requestTimeout">The maximum time to wait for server responses.</param>
        /// <param name="udpClient">The UdpClient instance used for sending and receiving UDP packets.</param>
        /// <param name="port">The UDP port to use for discovery (default is 3483).</param>
        /// <returns>A list of server response strings received during discovery.</returns>
        public static IReadOnlyList<MediaServer> Discover(CancellationToken cancellationToken, TimeSpan requestTimeout, IUdpClient udpClient, int port = 3483)
        {
            ArgumentNullException.ThrowIfNull(udpClient);

            var servers = new List<MediaServer>();

            using (udpClient)
            {
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));
                udpClient.EnableBroadcast = true;
                udpClient.Client.ReceiveTimeout = (int)requestTimeout.TotalMilliseconds;

                var data = Encoding.UTF8.GetBytes("eIPAD\0NAME\0VERS\0UUID\0JSON\0CLIP\0");
                udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Broadcast, port));

                while (true)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var from = new IPEndPoint(0, 0);
                        var recvBuffer = udpClient.Receive(ref from);
                        var response = Encoding.UTF8.GetString(recvBuffer);
                        if (string.IsNullOrWhiteSpace(response))
                        {
                            continue;
                        }
                        if (response == "eIPAD\0NAME\0VERS\0UUID\0JSON\0CLIP\0")
                        {
                            continue;
                        }
                        Dictionary<string, string> keyValuePairs;
                        var parsed = TryParse(recvBuffer, out keyValuePairs);
                        if (parsed)
                        {
                            var mediaServer = Map(keyValuePairs);
                            mediaServer.IPAddress = from.Address;
                            servers.Add(mediaServer);
                        }
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
            }

            return servers;
        }

        /// <summary>
        /// Maps a dictionary of key-value pairs to a <see cref="MediaServer"/> object.
        /// </summary>
        /// <returns>A <see cref="MediaServer"/> object populated with data extracted from the response string.</returns>
        private static MediaServer Map(IDictionary<string, string> keyValuePairs)
        {
            var server = new MediaServer
            {
                Name = keyValuePairs.TryGetValue("NAME", out var name) ? name : null,
                Version = keyValuePairs.TryGetValue("VERS", out var version) ? new Version(version) : null,
                UUID = keyValuePairs.TryGetValue("UUID", out var uuid) ? new Guid(uuid) : null,
                Json = keyValuePairs.TryGetValue("JSON", out var json) ? int.Parse(json) : null,
                Clip = keyValuePairs.TryGetValue("CLIP", out var clip) ? int.Parse(clip) : null
            };

            return server;
        }

        /// <summary>
        /// Parses a byte array response from the Logitech Media Server discovery process into a dictionary of key-value pairs.
        /// The response is expected to be in a specific format where each key is 4 bytes long, followed by a 4-byte length value, and then the value itself.
        /// Keys and values are UTF-8 encoded strings.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private static Dictionary<string, string> Parse(byte[] response)
        {
            var chunks = new List<Chunk>();

            for (int i = 0; i < response.Length; i++)
            {
                var chunk = new Chunk
                {
                    Value = response[i],
                    Index = i
                };

                if (chunk.Index == 0 && chunk.Value != 'E')
                {
                    throw new FormatException("Failed to parse discovery packet response due to invalid format. Expected response to start with 'E'.");
                }

                if (chunk.IsHandshakeStart)
                {
                    continue;
                }

                chunks.Add(chunk);
            }

            var keyValuePairs = new Dictionary<string, string>();

            while (chunks.Any(c => !c.HasBeenParsed))
            {
                var (key, value) = ExtractKeyValuePair(chunks);
                keyValuePairs[key] = value;
                chunks.RemoveAll(c => c.HasBeenParsed);
            }

            return keyValuePairs;
        }

        /// <summary>
        /// Attempts to parse a byte array response from a Logitech Media Server discovery packet into a dictionary of key-value pairs.
        /// </summary>
        /// <param name="response">The byte array containing the raw discovery response packet from the server.</param>
        /// <param name="result">When this method returns, contains the parsed key-value pairs if parsing was successful; otherwise, an empty dictionary.</param>
        /// <returns>
        /// <c>true</c> if the response was successfully parsed and contains at least one key-value pair; 
        /// <c>false</c> if parsing failed due to invalid format or if no key-value pairs were found.
        /// </returns>
        /// <remarks>
        /// This method provides a safe way to parse discovery responses without throwing exceptions.
        /// It uses the internal <see cref="Parse(byte[])"/> method to perform the actual parsing,
        /// catching any exceptions that may occur due to malformed packets or unexpected data formats.
        /// The method only returns <c>true</c> if parsing succeeds and at least one key-value pair is extracted,
        /// ensuring that the result contains meaningful data.
        /// </remarks>
        private static bool TryParse(byte[] response, out Dictionary<string, string> result)
        {
            result = new Dictionary<string, string>();
            try
            {
                result = Parse(response);
            }
            catch (Exception)
            {
                return false;
            }

            var keys = new List<string> { "NAME", "VERS", "UUID", "JSON", "CLIP" };

            var localKeys = result.Keys.ToList();
            var hasKeys = keys.All(k => localKeys.Contains(k));

            return hasKeys;
        }

        /// <summary>
        /// Extracts a key-value pair from the list of chunks.
        /// The key is expected to be 4 bytes long, followed by a 4-byte length value, and then the value itself.
        /// The method modifies the chunks list by marking the processed chunks as parsed.
        /// </summary>
        /// <param name="chunks"></param>
        /// <seealso href="https://github.com/LMS-Community/slimserver/blob/public/9.1/Slim/Networking/Discovery/Server.pm#L188"/>
        /// <returns></returns>
        private static (string, string) ExtractKeyValuePair(List<Chunk> chunks)
        {
            var keyBuffer = new byte[4];
            var valueBuffer = new List<char>();
            var lengthChunk = chunks[4];
            int? valueLength = lengthChunk.Value;
            lengthChunk.HasBeenParsed = true;
            var totalLength = valueLength + keyBuffer.Length + Chunk.Width;

            for (int i = 0; i < chunks.Count; i++)
            {
                if (i < keyBuffer.Length)
                {
                    keyBuffer[i] = chunks[i].Value;
                    chunks[i].HasBeenParsed = true;
                }

                if (i > 4 && i < totalLength)
                {
                    valueBuffer.Add(chunks[i].ParsedValue);
                    chunks[i].HasBeenParsed = true;
                }
            }

            return (Encoding.UTF8.GetString(keyBuffer), new string([.. valueBuffer]));
        }
    }
}
