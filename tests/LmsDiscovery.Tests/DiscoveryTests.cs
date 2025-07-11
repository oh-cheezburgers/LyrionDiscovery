using FluentAssertions;
using LmsDiscovery.UdpClient;
using Moq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LmsDiscovery.Tests
{
    public class DiscoveryTests
    {
        private Mock<IUdpClient> udpClientMock;
        private const string handshake = "eIPAD\0NAME\0VERS\0UUID\0JSON\0CLIP\0";

        public DiscoveryTests()
        {
            var socketMock = new Mock<ISocket>();
            udpClientMock = new Mock<IUdpClient>();
            socketMock.Setup(m => m.EnableBroadcast).Returns(true);
            udpClientMock.Setup(m => m.Client.ReceiveTimeout).Returns(1000);
            udpClientMock.Setup(m => m.Client).Returns(socketMock.Object);
        }

        private void MockSend()
        {
            udpClientMock.Setup(m => m.Send(Encoding.UTF8.GetBytes(handshake), Encoding.UTF8.GetBytes(handshake).Length, It.IsAny<IPEndPoint>()))
                         .Returns(Encoding.UTF8.GetBytes(handshake).Length);
        }

        [Fact]
        public void Discover_WhenValidResponseReceived_ReturnsDiscoveredServers()
        {
            //Arrange
            int callCount = 0;
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                .Returns((ref IPEndPoint ep) =>
                {
                    if (callCount == 0)
                    {
                        callCount++;
                        ep = new IPEndPoint(IPAddress.Parse("107.70.178.215"), 3483);
                        return Encoding.UTF8.GetBytes("ENAME\fMEDIA-SERVERVERS\u00059.0.2UUID$b34f68fa-e9ae-4238-b2ce-18bb48fa26a6JSON\u00049000CLIP\u00049090");
                    }
                    else
                    {
                        throw new OperationCanceledException();
                    }
                });
            var expected = new MediaServer
            {
                Name = "MEDIA-SERVER",
                Version = new Version("9.0.2"),
                UUID = new Guid("b34f68fa-e9ae-4238-b2ce-18bb48fa26a6"),
                Json = 9000,
                Clip = 9090,
                IPAddress = IPAddress.Parse("107.70.178.215")
            };

            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;

            //Act
            var response = Discovery.Discover(cancellationToken, TimeSpan.FromSeconds(1), udpClientMock.Object);

            //Assert
            response.Should().ContainEquivalentOf(expected);
            udpClientMock.Verify(m => m.Dispose(), Times.Once);
        }

        [Fact]
        public void Discover_RequestTimeoutExceeded_ReturnsEmptyList()
        {
            // Arrange
            MockSend();
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                         .Throws(new SocketException((int)SocketError.TimedOut));
            var expected = new List<string> { handshake };
            var cancellationToken = new CancellationTokenSource().Token;

            //Act
            var response = Discovery.Discover(cancellationToken, TimeSpan.FromSeconds(1), udpClientMock.Object);

            //Assert
            response.Should().BeEmpty();
        }
    }
}