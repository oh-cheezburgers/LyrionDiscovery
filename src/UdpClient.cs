using System.Net;
using System.Net.Sockets;

namespace LmsDiscovery;

/// <summary>
/// 
/// </summary>
public class UdpClient : System.Net.Sockets.UdpClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UdpClient"/> class with default settings.
    /// </summary>
    public UdpClient() : base()
    {
    }

    /// <inheritdoc cref="System.Net.Sockets.UdpClient.Client"/>
    public virtual new Socket Client
    {
        get => base.Client;
        set => base.Client = value;
    }

    /// <inheritdoc cref="System.Net.Sockets.UdpClient.EnableBroadcast"/>
    public virtual new bool EnableBroadcast
    {
        get => base.EnableBroadcast;
        set => base.EnableBroadcast = value;
    }

    /// <inheritdoc cref="System.Net.Sockets.UdpClient.Send(byte[], int , IPEndPoint)"/>
    public virtual new int Send(byte[] datagram, int bytes, IPEndPoint endPoint) => base.Send(datagram, bytes, endPoint);


    /// <inheritdoc cref="System.Net.Sockets.UdpClient.Receive(ref IPEndPoint)"/>
    public virtual new byte[] Receive(ref IPEndPoint endPoint) => base.Receive(ref endPoint);
}