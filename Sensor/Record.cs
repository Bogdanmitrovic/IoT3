using Newtonsoft.Json;

namespace Sensor;

public class Record
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    [JsonProperty("junction")]
    public int Junction { get; set; }
    [JsonProperty("vehicles")]
    public int Vehicles { get; set; }
    [JsonProperty("id")]
    public required string Id { get; set; }
    
}