using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace LyrionDiscovery.UdpClient;

/// <summary>
/// Provides a wrapper for the <see cref="Socket"/> class to facilitate testing.
/// </summary>
[ExcludeFromCodeCoverage]
public class SocketWrapper : ISocket, IDisposable
{
    private readonly Socket socket;
    private bool disposed;

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

    /// <inheritdoc/>
    public Socket GetUnderlyingSocket() => socket;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="SocketWrapper"/> class.
    /// </summary>
    /// <param name="disposing">True if called from Dispose; false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            socket.Dispose();
        }

        disposed = true;
    }
}