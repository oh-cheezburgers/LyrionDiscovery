using LyrionDiscovery.UdpClient;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace LyrionDiscovery;

/// <summary>
/// Provides a wrapper for the <see cref="UdpClient"/> class to facilitate testing.
/// </summary>
[ExcludeFromCodeCoverage]
public class UdpClientWrapper : IUdpClient
{
    private readonly System.Net.Sockets.UdpClient udpClient;

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
        get => new UdpClient.SocketWrapper(udpClient.Client); // Wrap the underlying Socket in a SocketWrapper
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
        udpClient.Dispose();
    }
}