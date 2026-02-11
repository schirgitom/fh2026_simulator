using System.Text;
using System.Text.Json;

namespace AquariumSimulator.Payload;

public sealed class HexPayloadEncoder : IPayloadEncoder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    public string Encode(Dictionary<string, double> payload)
    {
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToHexString(bytes);
    }
}
