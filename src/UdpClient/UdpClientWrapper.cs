using LyrionDiscovery.UdpClient;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace LyrionDiscovery;

/// <summary>
/// Provides a wrapper for the <see cref="UdpClient"/> class to facilitate testing.
/// </summary>
[ExcludeFromCodeCoverage]
public class UdpClientWrapper : IUdpClient, IDisposable
{
    private readonly System.Net.Sockets.UdpClient udpClient;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpClientWrapper"/> class with default settings.
    /// </summary>
    public UdpClientWrapper()
    {
        udpClient = new System.Net.Sockets.UdpClient();
    }

    /// <inheritdoc />
    public ISocket Client
    {
        get => new SocketWrapper(udpClient.Client);
        set => value.GetUnderlyingSocket();
    }

    /// <inheritdoc />
    public bool EnableBroadcast
    {
        get => udpClient.EnableBroadcast;
        set => udpClient.EnableBroadcast = value;
    }

    /// <inheritdoc />
    public int Send(byte[] datagram, int bytes, IPEndPoint endPoint) => udpClient.Send(datagram, bytes, endPoint);

    /// <inheritdoc />
    public byte[] Receive(ref IPEndPoint endPoint) => udpClient.Receive(ref endPoint);

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the resources used by the <see cref="UdpClientWrapper"/> class.
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
            udpClient.Dispose();
        }

        disposed = true;
    }
}