using System.Text;
using System.Text.Json.Serialization;

namespace FivemSdkGenerator.Objects;

public class NativeFunction : ICloneable
{
    public string Name { get; set; }
    
    [JsonPropertyName("params")]
    public NativeParameters[] Parameters { get; set; }
    public string Alias { get; set; }
    public string Results { get; set; }
    public string Description { get; set; }
    
    [JsonPropertyName("apiset")]
    public string ApiSet { get; set; }

    public string GetName()
    {
        if (!Name.Contains('_'))
            return Name[..1].ToUpper() + Name[1..];
        
        var builder = new StringBuilder();
        var split = Name.Split("_");
        foreach (var word in split)
        {
            if(string.IsNullOrEmpty(word))
                continue;
            
            var result = $"{word[..1].ToUpper()}{word[1..].ToLower()}";
            builder.Append(result);
        }

        return builder.ToString();
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}