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
            
            var from = new IPEndPoint(IPAddress.Any, 0);
            udpClientMock.Setup(m => m.Receive(ref It.Ref<IPEndPoint>.IsAny))
                .Callback(() => Thread.Sleep(1000))
                .Returns(Encoding.UTF8.GetBytes(handshake));

            udpClientMock.Setup(m => m.Send(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<IPEndPoint>()))
                .Returns(1);
        }

        [Fact]
        public void Discover()
        {
            //Arrange            
            var sut = new Discoverer();

            //Act
            var response = sut.Discover(TimeSpan.FromSeconds(1), udpClientMock.Object);

            //Assert
            response.Should().Contain(handshake);
        }
    }
}