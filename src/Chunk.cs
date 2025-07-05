using System.Text;

namespace LmsDiscovery;

public class Chunk
{
    public byte RawValue { get; set; }
    public char ParsedValue => Encoding.ASCII.GetString([RawValue]).FirstOrDefault();

    public int LengthValue => RawValue;

    public bool HasBeenParsed { get; set; } = false;

    public int? Index { get; set; } = null;

    public bool IsHandshakeStart => ParsedValue == 'E' && Index == 0;

    public int Width => 1; // Assuming each chunk is 1 byte wide
}