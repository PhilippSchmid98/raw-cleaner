namespace RawCleaner.Core.Models;

/// <summary>
/// The full result returned by <see cref="Services.IPhotoSyncService.AnalyzeFoldersAsync"/>.
/// </summary>
/// <param name="Statistics">Aggregate statistics for the two folders.</param>
/// <param name="Actions">Granular per-file list of planned actions.</param>
public sealed record AnalysisResult(SyncStatistics Statistics, IReadOnlyList<SyncAction> Actions);
