using System.Text;

namespace LmsDiscovery;

public class Chunk
{
    public byte Value { get; set; }

    public char ParsedValue => Encoding.ASCII.GetString([Value]).FirstOrDefault();

    public bool IsLengthValue => Index == 4;

    public bool HasBeenParsed { get; set; } = false;

    public int? Index { get; set; } = null;

    public bool IsHandshakeStart => ParsedValue == 'E' && Index == 0;

    public int Width => 1;
}