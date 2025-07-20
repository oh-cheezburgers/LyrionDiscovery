using System.Net;

namespace LyrionDiscovery
{
    /// <summary>
    /// Represents a Lyrion Music Server discovered on the local network.
    /// </summary>
    public class MusicServer
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

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="MusicServer"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object? obj)
        {
            if (obj is not MusicServer other) return false;

            return IPAddress?.Equals(other.IPAddress) == true &&
                   Name == other.Name &&
                   Version?.Equals(other.Version) == true &&
                   UUID?.Equals(other.UUID) == true &&
                   Json == other.Json &&
                   Clip == other.Clip;
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="MusicServer"/> instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(IPAddress, Name, Version, UUID, Json, Clip);
        }
    }
}