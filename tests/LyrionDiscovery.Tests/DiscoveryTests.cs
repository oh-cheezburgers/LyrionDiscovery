using FluentAssertions;
using LyrionDiscovery.UdpClient;
using Moq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LyrionDiscovery.Tests
{
    public class DiscoveryTests
    {
        private Mock<IUdpClient> udpClientMock;
        private const string discoveryPacket = "eIPAD\0NAME\0VERS\0UUID\0JSON\0CLIP\0";
        private const string validMediaServerResponse = "ENAME\fMEDIA-SERVERVERS\u00059.0.2UUID$b34f68fa-e9ae-4238-b2ce-18bb48fa26a6JSON\u00049000CLIP\u00049090";
        private const string validLivingRoomServerResponse = "ENAME\u0012LIVING-ROOM-SERVERVERS\u00059.0.2UUID$a12c34ef-b567-8901-d234-56ef78901234JSON\u00049000CLIP\u00049090";
        private const string invalidResponse = "InvalidResponseString";
        private const string reorderedResponse = "EJSON\u00049000CLIP\u00049090NAME\fMEDIA-SERVERUUID$b34f68fa-e9ae-4238-b2ce-18bb48fa26a6VERS\u00059.0.2";
        private const string missingKeyValuePairsResponse = "EJSON\u00049000CLIP\u00049090";
        private const string malformedResponse = "EThisIsAnInvalidResponse";
        private const string missingValueResponse = "EJSON\0CLIP\u00049090NAME\fMEDIA-SERVERUUID$b34f68fa-e9ae-4238-b2ce-18bb48fa26a6VERS\u00059.0.2";

        public DiscoveryTests()
        {
            var socketMock = new Mock<ISocket>();
            udpClientMock = new Mock<IUdpClient>(MockBehavior.Strict);
            udpClientMock.SetupProperty(m => m.EnableBroadcast);
            udpClientMock.Setup(m => m.Client.ReceiveTimeout).Returns(1000);
            udpClientMock.Setup(m => m.Client).Returns(socketMock.Object);
        }

        [Fact]
        public void Discover_ValidResponseReceived_ReturnsDiscoveredServer()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses("107.70.178.215", validMediaServerResponse));

            var expected = new MusicServer
            {
                Name = "MEDIA-SERVER",
                Version = new Version("9.0.2"),
                UUID = new Guid("b34f68fa-e9ae-4238-b2ce-18bb48fa26a6"),
                Json = 9000,
                Clip = 9090,
                IPAddress = IPAddress.Parse("107.70.178.215")
            };

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().ContainEquivalentOf(expected);
        }

        [Fact]
        public void Discover_ValidResponsesReceived_ReturnsDiscoveredServers()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses(
                ("107.70.178.215", validMediaServerResponse),
                ("230.186.191.21", validLivingRoomServerResponse)
            ));

            var servers = new List<MusicServer>()
            {
                new MusicServer
                {
                    Name = "MEDIA-SERVER",
                    Version = new Version("9.0.2"),
                    UUID = new Guid("b34f68fa-e9ae-4238-b2ce-18bb48fa26a6"),
                    Json = 9000,
                    Clip = 9090,
                    IPAddress = IPAddress.Parse("107.70.178.215")
                },
                new MusicServer
                {
                    Name = "LIVING-ROOM-SERVER",
                    Version = new Version("9.0.2"),
                    UUID = new Guid("a12c34ef-b567-8901-d234-56ef78901234"),
                    Json = 9000,
                    Clip = 9090,
                    IPAddress = IPAddress.Parse("230.186.191.21")
                }
            };

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().BeEquivalentTo(servers);
        }

        [Fact]
        public void Discover_ReceivesOnlyDiscoveryPackets_ReturnsEmptyList()
        {
            // Arrange
            MockSend();
            MockReceive(SetupReceiveResponses(
                ("107.70.178.215", discoveryPacket),
                ("230.186.191.21", discoveryPacket)
            ));

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public void Discover_WhenCalled_AlwaysCallsDispose()
        {
            // Arrange
            MockSend();
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                         .Throws(new SocketException((int)SocketError.TimedOut));

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), cancellationToken);

            // Assert
            udpClientMock.Verify(m => m.Dispose(), Times.Once);
        }

        [Fact]
        public void Discover_NoResponseReceived_ReturnsEmptyList()
        {
            // Arrange
            MockSend();
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                         .Throws(new SocketException((int)SocketError.TimedOut));

            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), cancellationToken);

            // Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public void Discover_TimeoutReached_CompletesDiscovery()
        {
            // Arrange
            MockSend();
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                         .Throws(new SocketException((int)SocketError.TimedOut));

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource().Token);

            //Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public void Discover_CancellationTokensCancellationRequested_CompletesDiscovery()
        {
            // Arrange
            MockSend();
            var cancellationToken = new CancellationTokenSource();
            MockReceive((ref IPEndPoint ep) =>
            {
                cancellationToken.Cancel();
                ep = new IPEndPoint(IPAddress.Parse("107.70.178.215"), 3483);
                return Encoding.UTF8.GetBytes(discoveryPacket);
            });

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.MaxValue, cancellationToken.Token);

            //Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public void Discover_InValidResponsesReceived_ReturnsDiscoveredServer()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses(
                ("107.70.178.215", validMediaServerResponse),
                ("230.186.191.21", invalidResponse)
            ));
            var servers = new List<MusicServer>()
            {
                new MusicServer
                {
                    Name = "MEDIA-SERVER",
                    Version = new Version("9.0.2"),
                    UUID = new Guid("b34f68fa-e9ae-4238-b2ce-18bb48fa26a6"),
                    Json = 9000,
                    Clip = 9090,
                    IPAddress = IPAddress.Parse("107.70.178.215")
                },
            };

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().BeEquivalentTo(servers);
        }

        [Fact]
        public void Discover_ResponseInDifferentOrder_ReturnsDiscoveredServer()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses("107.70.178.215", reorderedResponse));

            var servers = new List<MusicServer>()
            {
                new MusicServer
                {
                    Name = "MEDIA-SERVER",
                    Version = new Version("9.0.2"),
                    UUID = new Guid("b34f68fa-e9ae-4238-b2ce-18bb48fa26a6"),
                    Json = 9000,
                    Clip = 9090,
                    IPAddress = IPAddress.Parse("107.70.178.215")
                },
            };

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().BeEquivalentTo(servers);
        }

        [Fact]
        public void Discover_ResponseMissingKeyValuePairs_ReturnsEmptyList()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses("107.70.178.215", missingKeyValuePairsResponse));

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public void Discover_EmptyResponseReceived_ReturnsEmptyList()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses("127.0.0.1", string.Empty));

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public void Discover_MalformedResponseWithValidFirstCharacterReceived_ReturnsEmptyResponse()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses("127.0.0.1", malformedResponse));

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().BeEmpty();
        }

        [Fact]
        public void Discover_ResponseMissingValue_ReturnsDiscoveredServer()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses("107.70.178.215", missingValueResponse));

            var servers = new List<MusicServer>()
            {
                new MusicServer
                {
                    Name = "MEDIA-SERVER",
                    Version = new Version("9.0.2"),
                    UUID = new Guid("b34f68fa-e9ae-4238-b2ce-18bb48fa26a6"),
                    Clip = 9090,
                    IPAddress = IPAddress.Parse("107.70.178.215")
                },
            };

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().BeEquivalentTo(servers);
        }

        [Fact]
        public void Discover_SameServerMultipleResponses_ReturnsOnlyOneInstanceOfServer()
        {
            //Arrange
            MockSend();
            MockReceive(SetupReceiveResponses(
                ("107.70.178.215", validMediaServerResponse),
                ("107.70.178.215", validMediaServerResponse)
            ));

            var expectedServer = new MusicServer
            {
                Name = "MEDIA-SERVER",
                Version = new Version("9.0.2"),
                UUID = new Guid("b34f68fa-e9ae-4238-b2ce-18bb48fa26a6"),
                Json = 9000,
                Clip = 9090,
                IPAddress = IPAddress.Parse("107.70.178.215")
            };

            //Act
            var response = new Discovery().Discover(udpClientMock.Object, TimeSpan.FromSeconds(1), new CancellationTokenSource(TimeSpan.FromSeconds(1)).Token);

            //Assert
            response.Should().HaveCount(1);
            response.Should().ContainEquivalentOf(expectedServer);
        }

        private void MockSend()
        {
            udpClientMock.Setup(m => m.Send(Encoding.UTF8.GetBytes(discoveryPacket), Encoding.UTF8.GetBytes(discoveryPacket).Length, It.IsAny<IPEndPoint>()))
                         .Returns(Encoding.UTF8.GetBytes(discoveryPacket).Length);

            udpClientMock.Setup(m => m.Dispose());
        }

        private delegate byte[] MockReceiveDelegate(ref IPEndPoint endPoint);

        private void MockReceive(MockReceiveDelegate mockReceiveDelegate)
        {
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                .Returns((ref IPEndPoint endPoint) =>
                {
                    return mockReceiveDelegate(ref endPoint);
                });
        }

        private static MockReceiveDelegate SetupReceiveResponses(string ip, string response)
        {
            const int port = 3483;
            int callCount = 0;

            return (ref IPEndPoint ep) =>
            {
                if (callCount++ == 0)
                {
                    ep = new IPEndPoint(IPAddress.Parse(ip), port);
                    return Encoding.UTF8.GetBytes(response);
                }

                throw new OperationCanceledException();
            };
        }

        private static MockReceiveDelegate SetupReceiveResponses(params (string ip, string response)[] responses)
        {
            const int port = 3483;
            int callCount = 0;

            return (ref IPEndPoint ep) =>
            {
                if (callCount < responses.Length)
                {
                    var (ip, response) = responses[callCount++];
                    ep = new IPEndPoint(IPAddress.Parse(ip), port);
                    return Encoding.UTF8.GetBytes(response);
                }

                throw new SocketException((int)SocketError.TimedOut);
            };
        }
    }
}