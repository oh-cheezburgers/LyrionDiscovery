using FluentAssertions;
using LmsDiscovery.UdpClient;
using Moq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LmsDiscovery.Tests
{
    public class DiscoveryShould
    {
        private Mock<IUdpClient> udpClientMock;
        private const string handshake = "eIPAD\0NAME\0VERS\0UUID\0JSON\0CLIP\0";

        public DiscoveryShould()
        {
            var socketMock = new Mock<ISocket>();
            udpClientMock = new Mock<IUdpClient>();
            socketMock.Setup(m => m.EnableBroadcast).Returns(true);
            udpClientMock.Setup(m => m.Client.ReceiveTimeout).Returns(1000);
            udpClientMock.Setup(m => m.Client).Returns(socketMock.Object);
        }

        private void MockSend()
        {
            udpClientMock.Setup(m => m.Send(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IPEndPoint>()))
                         .Returns(1);
        }

        private void MockReceive(string handshake)
        {
            int callCount = 0;

            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                .Returns((ref IPEndPoint ep) =>
                {
                    if (callCount == 0)
                    {
                        callCount++;
                        ep = new IPEndPoint(IPAddress.Parse("107.70.178.215"), 3483);
                        return Encoding.UTF8.GetBytes(handshake);
                    }
                    else
                    {
                        throw new OperationCanceledException();
                    }
                });
        }

        [Fact]
        public void Discover()
        {
            //Arrange
            MockSend();
            MockReceive(handshake);
            var expected = new List<MediaServer>
            {
                new MediaServer()
            };
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;

            //Act
            var response = Discovery.Discover(cancellationToken, TimeSpan.FromSeconds(1), udpClientMock.Object);

            //Assert
            response.Should().BeEquivalentTo(expected);
            udpClientMock.Verify(m => m.Dispose(), Times.Once);
        }

        [Fact]
        public void Map()
        {
            //Arrange
            var expected = new MediaServer
            {
                Name = "MEDIA-SERVER",
                Version = new Version("9.0.2"),
                UUID = new Guid("b34f68fa-e9ae-4238-b2ce-18bb48fa26a6"),
                Json = 9000,
                Clip = 9090
            };

            var parsedResponse = new Dictionary<string, string>
            {
                { "NAME", expected.Name },
                { "VERS", expected.Version.ToString() },
                { "UUID", expected.UUID.ToString()! },
                { "JSON", expected.Json.ToString()! },
                { "CLIP", expected.Clip.ToString()! }
            };

            //Act
            var result = Discovery.Map(parsedResponse);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Parse()
        {
            //Arrange
            byte[] response = [69, 78, 65, 77, 69, 12, 77, 69, 68, 73, 65, 45, 83, 69, 82, 86, 69, 82, 86, 69, 82, 83, 5, 57, 46, 48, 46, 50,
                               85, 85, 73, 68, 36, 98, 51, 52, 102, 54, 56, 102, 97, 45, 101, 57, 97, 101, 45, 52, 50, 51, 56, 45, 98, 50, 99, 101,
                               45, 49, 56, 98, 98, 52, 56, 102, 97, 50, 54, 97, 54, 74, 83, 79, 78, 4, 57, 48, 48, 48, 67, 76, 73, 80, 4, 57, 48, 57, 48];
            var expected = new Dictionary<string, string>
            {
                { "NAME", "MEDIA-SERVER" },
                { "VERS", "9.0.2" },
                { "UUID", "b34f68fa-e9ae-4238-b2ce-18bb48fa26a6" },
                { "JSON", "9000" },
                { "CLIP", "9090" }
            };

            //Act
            var result = Discovery.Parse(response);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void HandleRequestTimeout()
        {
            // Arrange
            MockSend();
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                         .Throws(new SocketException((int)SocketError.TimedOut));
            var expected = new List<string> { handshake };
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token;

            //Act
            var response = Discovery.Discover(cancellationToken, TimeSpan.FromSeconds(1), udpClientMock.Object);

            //Assert
            response.Should().BeEmpty();
        }
    }
}