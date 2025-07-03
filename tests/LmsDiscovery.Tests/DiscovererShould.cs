using FluentAssertions;
using Moq;
using System.Net;
using System.Text;

namespace LmsDiscovery.Tests
{
    public class DiscovererShould
    {
        private Mock<UdpClient> udpClientMock;
        private const string handshake = "eIPAD\0NAME\0VERS\0UUID\0JSON\0CLIP\0";

        public DiscovererShould()
        {
            udpClientMock = new Mock<UdpClient>();
            udpClientMock.CallBase = true;
        }

        private void MockSend()
        {
            udpClientMock.Setup(m => m.Send(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IPEndPoint>()))
                            .Returns(1);
        }

        private void MockReceive(string handshake)
        {
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                            .Callback(() => Thread.Sleep(1000))
                            .Returns(Encoding.UTF8.GetBytes(handshake));
        }

        [Fact]
        public void Discover()
        {
            //Arrange
            MockSend();
            MockReceive(handshake);
            var expected = new List<string> { handshake };

            //Act
            var response = Discoverer.Discover(TimeSpan.FromSeconds(1), udpClientMock.Object);

            //Assert
            response.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Map()
        {
            //Arrange
            var expected = new MediaServer
            {
                Name = "MEDIA-SERVER",
                Version = "9.0.2",
                UUID = "b34f68fa-e9ae-4238-b2ce-18bb48fa26a6",
                Json = "9000",
                Clip = "9090"
            };
            var response = "ENAMEMEDIA-SERVERVERS9.0.2UUID$b34f68fa-e9ae-4238-b2ce-18bb48fa26a6JSON9000CLIP9090";

            //Act
            var result = Discoverer.Map(response);

            //Assert
            result.Should().BeEquivalentTo(expected);
        }
    }
}