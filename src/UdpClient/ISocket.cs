using System.Net;
using System.Net.Sockets;

namespace LyrionDiscovery.UdpClient;

/// <summary>
/// An abstraction of the <see cref="Socket"/> class to facilitate testing and abstraction.
/// This interface allows for mocking and testing without relying on the actual network stack.
/// </summary>
public interface ISocket : IDisposable
{
    /// <summary>
    /// Gets or sets a value indicating whether the socket should enable broadcast.
    /// </summary>
    bool EnableBroadcast { get; set; }

    /// <summary>
    /// Binds the socket to a local endpoint.
    /// </summary>
    /// <param name="localEP">The local endpoint to bind the socket to.</param>
    void Bind(IPEndPoint localEP);

    /// <summary>
    /// Gets or sets the amount of time, in milliseconds, that a receive operation blocks waiting for data.
    /// </summary>
    int ReceiveTimeout { get; set; }

    /// <summary>
    /// Gets the underlying <see cref="Socket"/> instance.
    /// </summary>
    /// <returns>The <see cref="Socket"/> object.</returns>
    public Socket GetUnderlyingSocket();
}