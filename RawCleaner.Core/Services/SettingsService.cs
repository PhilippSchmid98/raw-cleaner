using System.Text.Json;
using RawCleaner.Core.Models;

namespace RawCleaner.Core.Services;

/// <summary>
/// Loads and persists <see cref="AppSettings"/> as JSON in the user's AppData folder.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private static readonly string SettingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RawCleaner",
        "settings.json");

    private static readonly JsonSerializerOptions SerializerOptions =
        new() { WriteIndented = true };

    public AppSettings Current { get; private set; }

    public SettingsService()
    {
        Current = TryLoad() ?? AppSettings.Default;
    }

    private static AppSettings? TryLoad()
    {
        try
        {
            if (!File.Exists(SettingsFilePath)) return null;
            var json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json);
        }
        catch
        {
            return null;
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsFilePath)!);
        File.WriteAllText(
            SettingsFilePath,
            JsonSerializer.Serialize(settings, SerializerOptions));
        Current = settings;
    }

    public void Reset()
    {
        Save(AppSettings.Default);
    }
}
