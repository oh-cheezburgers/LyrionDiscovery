namespace LmsDiscovery
{
    /// <summary>
    /// Represents a Logitech Media Server discovered on the local network.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Gets or sets the name of the server.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of the server software.
        /// </summary>
        public required string Version { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier (UUID) of the server.
        /// </summary>
        public required string UUID { get; set; }

        /// <summary>
        /// Gets or sets the JSON port used by the server.
        /// </summary>
        public required string Json { get; set; }

        /// <summary>
        /// Gets or sets the CLI port used by the server.
        /// </summary>
        public required string Clip { get; set; }
    }
}