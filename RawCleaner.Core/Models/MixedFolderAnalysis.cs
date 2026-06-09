namespace RawCleaner.Core.Models;

/// <summary>
/// Result of scanning a single folder for mixed JPEG + RAW content.
/// </summary>
/// <param name="JpegCount">Number of JPEG files found.</param>
/// <param name="RawCount">Number of RAW files found.</param>
/// <param name="RawFiles">The RAW <see cref="PhotoFile"/> entries (candidates for moving).</param>
public sealed record MixedFolderAnalysis(
    int JpegCount,
    int RawCount,
    IReadOnlyList<PhotoFile> RawFiles)
{
    /// <summary>True when the folder contains at least one JPEG and one RAW file.</summary>
    public bool IsMixed => JpegCount > 0 && RawCount > 0;

    /// <summary>Empty sentinel returned when no analysis has been run yet.</summary>
    public static MixedFolderAnalysis Empty { get; } =
        new(0, 0, Array.Empty<PhotoFile>());
}
