namespace FivemSdkGenerator.Objects;

public class NativeParameters
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }

    public string GetName()
        => IsReservedLuaKeyword(Name)
            ? $"_{Name}"
            : Name;

    public string GetParsedType()
        => GetParsedType(Type);

    public static string GetParsedType(string value)
    {
        var raw = value.Replace("*", "");
        return raw switch
        {
            "int" or "float" or "long" => "number",
            "BOOL" => "boolean",
            "char" => "string",
            "Vector3" => "vector3",
            "Any" => "any",
            "void" => string.Empty,
            _ => raw
        };
    }
    
    private static bool IsReservedLuaKeyword(string value)
        => Application.ReservedLuaKeywords.Any(x => x.Equals(value));
}