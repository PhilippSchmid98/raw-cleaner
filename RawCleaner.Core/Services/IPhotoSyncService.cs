using RawCleaner.Core.Models;

namespace RawCleaner.Core.Services;

/// <summary>
/// Analyses two photo folders, executes cleanup and exports a report.
/// </summary>
public interface IPhotoSyncService
{
    /// <summary>
    /// Scans <paramref name="jpegFolderPath"/> and <paramref name="rawFolderPath"/>,
    /// matches files by base name (case-insensitive, extension-agnostic) and returns
    /// aggregate statistics together with a per-file action plan.
    /// </summary>
    /// <param name="jpegFolderPath">Absolute path to the JPEG folder.</param>
    /// <param name="rawFolderPath">Absolute path to the RAW folder.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<AnalysisResult> AnalyzeFoldersAsync(
        string jpegFolderPath,
        string rawFolderPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all RAW files that were flagged as <see cref="ActionType.DeleteRaw"/>
    /// by the most recent call to <see cref="AnalyzeFoldersAsync"/>.
    /// Must be called after <see cref="AnalyzeFoldersAsync"/>.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task ExecuteCleanupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes a CSV report to <paramref name="exportFilePath"/> containing
    /// the analysis statistics and the per-file action log.
    /// Must be called after <see cref="AnalyzeFoldersAsync"/>.
    /// </summary>
    /// <param name="exportFilePath">
    /// Full path (including file name) for the output CSV file.
    /// </param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task ExportReportAsync(
        string exportFilePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans a single folder for both JPEG and RAW files to detect mixed content.
    /// Use the result to decide whether to offer the "move RAWs" workflow.
    /// </summary>
    /// <param name="folderPath">Absolute path to the folder to scan.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task<MixedFolderAnalysis> AnalyzeMixedFolderAsync(
        string folderPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves all RAW files discovered by the most recent
    /// <see cref="AnalyzeMixedFolderAsync"/> call to <paramref name="targetFolderPath"/>,
    /// creating the folder if it does not exist.
    /// Must be called after <see cref="AnalyzeMixedFolderAsync"/>.
    /// </summary>
    /// <param name="targetFolderPath">Destination directory for the RAW files.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task ExecuteMoveAsync(
        string targetFolderPath,
        CancellationToken cancellationToken = default);
}
