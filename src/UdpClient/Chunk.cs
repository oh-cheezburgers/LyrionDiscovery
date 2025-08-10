using System.Text;

namespace LyrionDiscovery.UdpClient;

/// <summary>
/// Represents a chunk of data used in the discovery process.
/// Each chunk contains a byte value, its parsed character representation,
/// and metadata indicating whether it has been parsed, its index, and whether it is the start
/// </summary>
public class Chunk
{
    /// <summary>
    /// The byte value of the chunk.
    /// </summary>
    public byte Value { get; set; }

    /// <summary>
    /// The parsed character representation of the chunk's byte value.
    /// This is derived from the byte value using ASCII encoding.
    /// </summary>
    public char ParsedValue => Encoding.UTF8.GetString([Value]).FirstOrDefault();

    /// <summary>
    /// Indicates whether the chunk is a length value.
    /// A length value is expected to be at index 4 in the sequence of chunks.
    /// </summary>
    public bool IsLengthValue => Index == 4;

    /// <summary>
    /// Indicates whether the chunk has been parsed.
    /// This property is used to track whether the chunk has been processed in the discovery sequence.
    /// </summary>
    public bool HasBeenParsed { get; set; }

    /// <summary>
    /// The index of the chunk in the sequence.
    /// This is used to determine the order of chunks and to identify special cases like the handshake
    /// </summary>
    public int? Index { get; set; }

    /// <summary>
    /// Indicates whether the chunk is the start of a handshake.
    /// The handshake start is identified by the character 'E' at index 0.
    /// Upper-case 'E' for response packet appears to be a protocol convention.
    /// </summary>
    public bool IsHandshakeStart => ParsedValue == 'E' && Index == 0;

    /// <summary>
    /// The width of the chunk, which is always 1 byte.    
    /// </summary>
    public static int Width => 1;
}