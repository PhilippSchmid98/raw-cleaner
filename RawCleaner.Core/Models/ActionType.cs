namespace RawCleaner.Core.Models;

/// <summary>
/// Describes the planned or executed action for a single file during cleanup.
/// </summary>
public enum ActionType
{
    /// <summary>A matching JPEG exists – the RAW file is kept.</summary>
    KeepMatched,

    /// <summary>No matching JPEG found – the RAW file will be deleted.</summary>
    DeleteRaw,

    /// <summary>No matching RAW found – the JPEG is orphaned (kept, no action).</summary>
    OrphanedJpeg
}
