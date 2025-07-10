using System.Net;

namespace LmsDiscovery
{
    /// <summary>
    /// Represents a Logitech Media Server discovered on the local network.
    /// </summary>
    public class MediaServer
    {
        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the version of the server software.
        /// </summary>
        public Version? Version { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier (UUID) of the server.
        /// </summary>
        public Guid? UUID { get; set; }

        /// <summary>
        /// Gets or sets the JSON port used by the server.
        /// </summary>
        public int? Json { get; set; }

        /// <summary>
        /// Gets or sets the CLI port used by the server.
        /// </summary>
        public int? Clip { get; set; }

        /// <summary>
        /// Gets or sets the IP address of the server.
        /// </summary>
        public IPAddress? IPAddress { get; set; }
    }
}