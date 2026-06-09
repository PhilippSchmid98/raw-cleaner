namespace RawCleaner.Core.Models;

/// <summary>
/// User-configurable settings for RawCleaner, persisted as JSON.
/// </summary>
public sealed class AppSettings
{
    /// <summary>A fresh instance pre-loaded with sensible defaults.</summary>
    public static AppSettings Default => new();

    /// <summary>
    /// File extensions recognised as RAW files (case-insensitive).
    /// </summary>
    public List<string> RawExtensions { get; set; } =
    [
        ".raf", ".cr3", ".cr2", ".nef", ".arw", ".dng", ".rw2", ".orf",
        ".pef", ".srw", ".x3f", ".3fr", ".mef", ".mrw", ".rwl", ".srf",
        ".erf", ".kdc", ".dcr", ".raw", ".rwz", ".iiq"
    ];

    /// <summary>
    /// Folder names checked (in order) when auto-detecting the RAW subfolder
    /// next to a selected JPEG folder. First match wins.
    /// </summary>
    public List<string> RawFolderCandidates { get; set; } =
    [
        "RAW", "Raw", "raw", "Raws", "RAWS", "RAW_files", "raw_files",
        "NEF", "CR3", "ARW", "RAF"
    ];

    /// <summary>
    /// Default subfolder name appended to the source folder when moving
    /// RAW files out of a mixed folder.
    /// </summary>
    public string DefaultRawSubfolderName { get; set; } = "RAW";
}
