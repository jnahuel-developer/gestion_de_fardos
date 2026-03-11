using System.Text.Json;
using GestionDeFardos.Core.Config;
using GestionDeFardos.Core.Interfaces;

namespace GestionDeFardos.Infrastructure.Config;

public sealed class ConfigLoader : IConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AppSettings Load(string baseDirectory)
    {
        var configPath = Path.Combine(baseDirectory, "config.json");
        if (!File.Exists(configPath))
        {
            return new AppSettings();
        }

        var json = File.ReadAllText(configPath);
        var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
        return settings ?? new AppSettings();
    }
}
