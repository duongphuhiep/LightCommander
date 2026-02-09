namespace Api;

using System.Text.Json.Serialization;

public class LightModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("is_on")]
    public bool IsOn { get; set; }
    
    [JsonPropertyName("temperature")]
    public long? LightTemperature { get; set; }
}