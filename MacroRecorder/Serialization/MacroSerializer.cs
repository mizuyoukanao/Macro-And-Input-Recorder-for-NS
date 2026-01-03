using System.Text.Json;
using System.Text.Json.Serialization;
using MacroRecorder.Models;

namespace MacroRecorder.Serialization;

public static class MacroSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void Write(string path, MacroDefinition macro)
    {
        var json = JsonSerializer.Serialize(macro, Options);
        File.WriteAllText(path, json);
    }

    public static MacroDefinition Read(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<MacroDefinition>(json, Options) ?? new MacroDefinition();
    }
}
