namespace RawCleaner.Core.Models;

/// <summary>
/// Represents a single photo file discovered on disk.
/// </summary>
/// <param name="FullPath">Absolute path to the file.</param>
/// <param name="BaseName">File name without extension (used for matching).</param>
/// <param name="Type">Whether the file is a JPEG or a RAW image.</param>
public sealed record PhotoFile(string FullPath, string BaseName, FileType Type);
