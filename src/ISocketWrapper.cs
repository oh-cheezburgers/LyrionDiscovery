using System.Net;
using System.Net.Sockets;

namespace LmsDiscovery;

/// <summary>
/// Defines an interface for a socket wrapper that provides basic socket operations and properties.
/// </summary>
public interface ISocketWrapper : IDisposable
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
    /// Gets the underlying raw <see cref="Socket"/> instance.
    /// </summary>
    /// <returns>The raw <see cref="Socket"/> object.</returns>
    public Socket GetUnderlyingSocket();
}