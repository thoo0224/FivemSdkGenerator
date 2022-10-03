using System.Text;
using System.Text.Json;
using FivemSdkGenerator.Objects;

namespace FivemSdkGenerator;

public class Application
{
    public static readonly string[] ReservedLuaKeywords =
    {
        "and", "break", "do", "else", "elseif",
        "end", "false", "for", "function", "if",
        "in", "local", "nil", "not", "or",
        "repeat", "return", "then", "true", "until", "while",
    };
    
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
    
    public static async Task StartAsync()
    {
        var cfx = await GetNativesAsync("https://runtime.fivem.net/doc/natives_cfx.json");
        var natives = await GetNativesAsync("https://runtime.fivem.net/doc/natives.json");

        foreach (var (moduleName, functions) in natives)
            await CreateModuleAsync(moduleName, Path.Combine("sdk", "fivem"), functions.Values);
        
        foreach (var (moduleName, functions) in cfx)
            await CreateModuleAsync(moduleName, Path.Combine("sdk", "cfx"), functions.Values);

        var staticFunctionsContent = await File.ReadAllTextAsync(Path.Combine("Resources", "static_functions.json"));
        var staticFunctions = JsonSerializer.Deserialize<NativeFunction[]>(staticFunctionsContent, JsonSerializerOptions);
        await CreateModuleAsync("static", Path.Combine("sdk", "cfx"), staticFunctions);
    }

    private static async Task CreateModuleAsync(string moduleName, string dir, IEnumerable<NativeFunction> functions)
    {
        Directory.CreateDirectory(dir);
        
        var moduleBuilder = new StringBuilder();
        foreach (var function in functions.SelectMany(x =>
                 {
                     var result = new List<NativeFunction>();
                     result.Add(x);

                     if (x.Alias != null)
                     {
                         var alias = (NativeFunction)x.Clone();
                         alias.Name = alias.Alias;
                         result.Add(alias);
                     }

                     return result; 
                 }))
        {
            var functionBuilder = GenerateFunctionDeclaration(function, moduleName);
            if (functionBuilder == null)
                continue;

            moduleBuilder.Append(functionBuilder);
            moduleBuilder.AppendLine();
        }

        await File.WriteAllTextAsync(Path.Combine(dir, $"{moduleName.ToLower()}.lua"), moduleBuilder.ToString());
    }
    
    private static async Task<Dictionary<string, Dictionary<string, NativeFunction>>> GetNativesAsync(string url)
    {
        var client = new HttpClient();
        var stream = await client.GetStreamAsync(url);
        var response = await JsonSerializer.DeserializeAsync<Dictionary<string, Dictionary<string, NativeFunction>>>(stream, JsonSerializerOptions);

        return response;
    }
    
    private static StringBuilder GenerateFunctionDeclaration(NativeFunction function, string module)
    {
        if (string.IsNullOrEmpty(function.Name))
            return null;

        var sb = new StringBuilder();
        AppendFunctionDocumentation(function, sb, module);
        AppendFunctionDeclaration(function, sb);

        return sb;
    }

    private static void AppendFunctionDeclaration(NativeFunction function, StringBuilder sb)
    {
        var parameters = function.Parameters is { Length: > 0 }
            ? $"({string.Join(", ", function.Parameters.Select(x => x.GetName()))})"
            : "()";
        sb.AppendLine($"function {function.GetName()}{parameters} end");
    }

    private static void AppendFunctionDocumentation(NativeFunction function, StringBuilder sb, string module)
    {
        if (function.Description != null)
            sb.AppendLine($"-- {function.Description.Replace("\n", "\n-- ")}");

        foreach (var parameter in function.Parameters)
            sb.AppendLine($"-- @param {parameter.GetName()} {parameter.GetParsedType()}");

        sb.AppendLine($"-- @return {NativeParameters.GetParsedType(function.Results)}");
    }
}