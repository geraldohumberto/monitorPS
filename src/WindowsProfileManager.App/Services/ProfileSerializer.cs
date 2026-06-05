using System.Text.Json;
using WindowsProfileManager.App.Models;

namespace WindowsProfileManager.App.Services;

public sealed class ProfileSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Save(string filePath, WindowsProfile profile)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(profile, Options));
    }

    public WindowsProfile Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<WindowsProfile>(json, Options) ?? new WindowsProfile();
    }
}
