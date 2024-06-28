using Newtonsoft.Json;

namespace Filter;

public class AverageRecord
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    [JsonProperty("vehicles")]
    public double Vehicles { get; set; }
    [JsonProperty("id")]
    public required string Id { get; set; }
}