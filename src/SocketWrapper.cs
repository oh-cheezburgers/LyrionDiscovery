using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace LmsDiscovery;

/// <summary>
/// Provides a wrapper around the <see cref="Socket"/> class to facilitate abstraction and testing.
/// </summary>
[ExcludeFromCodeCoverage]
public class SocketWrapper : ISocketWrapper, IDisposable
{
    private readonly Socket socket;

    /// <summary>
    /// Initializes a new instance of the <see cref="SocketWrapper"/> class with the specified <see cref="Socket"/>.
    /// </summary>
    /// <param name="socket">The underlying <see cref="Socket"/> instance to wrap.</param>
    public SocketWrapper(Socket socket)
    {
        this.socket = socket;
    }

    /// <inheritdoc/>
    public bool EnableBroadcast
    {
        get => socket.EnableBroadcast;
        set => socket.EnableBroadcast = value;
    }

    /// <inheritdoc/>
    public int ReceiveTimeout
    {
        get => socket.ReceiveTimeout;
        set => socket.ReceiveTimeout = value;
    }

    /// <inheritdoc/>
    public void Bind(IPEndPoint localEP)
    {
        socket.Bind(localEP);
    }

    /// <summary>
    /// Gets the underlying raw <see cref="Socket"/> instance.
    /// </summary>
    /// <returns>The wrapped <see cref="Socket"/>.</returns>
    public Socket GetUnderlyingSocket() => socket;

    /// <inheritdoc/>
    public void Dispose()
    {
        socket.Dispose();
    }
}