namespace AquariumSimulator.Payload;

public interface IPayloadEncoder
{
    string Encode(Dictionary<string, double> payload);
}
