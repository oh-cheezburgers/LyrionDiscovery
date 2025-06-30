using System.Net;
using System.Net.Sockets;

namespace LmsDiscovery;

public class UdpClient : System.Net.Sockets.UdpClient
{
    public UdpClient() : base()
    {
    }

    public virtual new Socket Client
    {
        get => base.Client;
        set => base.Client = value;
    }

    public virtual new bool EnableBroadcast
    {
        get => base.EnableBroadcast;
        set => base.EnableBroadcast = value;
    }

    public virtual new int Send(byte[] datagram, int bytes, IPEndPoint endPoint) => base.Send(datagram, bytes, endPoint);    

    public virtual new byte[] Receive(ref IPEndPoint endPoint) => base.Receive(ref endPoint);    
}