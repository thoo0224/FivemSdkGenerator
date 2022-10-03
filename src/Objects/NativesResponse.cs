using System.Text.Json.Serialization;

namespace FivemSdkGenerator.Objects;

public class NativesResponse
{
    [JsonPropertyName("CFX")]
    public Dictionary<string, NativeFunction> Functions { get; set; }
}