using RawCleaner.Core.Models;

namespace RawCleaner.Core.Services;

/// <summary>
/// Persists and provides access to <see cref="AppSettings"/>.
/// </summary>
public interface ISettingsService
{
    /// <summary>The currently active settings.</summary>
    AppSettings Current { get; }

    /// <summary>Persists <paramref name="settings"/> and makes them the active settings.</summary>
    void Save(AppSettings settings);

    /// <summary>Resets to factory defaults and persists them.</summary>
    void Reset();
}
