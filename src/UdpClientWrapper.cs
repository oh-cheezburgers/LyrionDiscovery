using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace LmsDiscovery;

/// <summary>
/// Provides a wrapper for the <see cref="UdpClient"/> class, implementing <see cref="IUdpClient"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class UdpClientWrapper : IUdpClient
{

    private readonly UdpClient udpClient;
    /// <summary>
    /// Initializes a new instance of the <see cref="UdpClientWrapper"/> class with default settings.
    /// </summary>
    public UdpClientWrapper()
    {
        udpClient = new UdpClient();
    }

    /// <inheritdoc cref="UdpClient.Client"/>
    public ISocketWrapper Client
    {
        get => new SocketWrapper(udpClient.Client); // Wrap the underlying Socket in a SocketWrapper
        set => value.GetUnderlyingSocket();
    }

    /// <inheritdoc cref="UdpClient.EnableBroadcast"/>
    public bool EnableBroadcast
    {
        get => udpClient.EnableBroadcast;
        set => udpClient.EnableBroadcast = value;
    }

    /// <inheritdoc cref="UdpClient.Send(byte[], int , IPEndPoint)"/>
    public int Send(byte[] datagram, int bytes, IPEndPoint endPoint) => udpClient.Send(datagram, bytes, endPoint);


    /// <inheritdoc cref="UdpClient.Receive(ref IPEndPoint)"/>
    public byte[] Receive(ref IPEndPoint endPoint) => udpClient.Receive(ref endPoint);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        udpClient.Dispose();
    }
}