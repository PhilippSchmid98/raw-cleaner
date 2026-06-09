using RawCleaner.Core.Models;
using System.Text;

namespace RawCleaner.Core.Services;

public sealed class PhotoSyncService : IPhotoSyncService
{
    private static readonly HashSet<string> JpegExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".jpe", ".jfif" };

    private readonly ISettingsService _settingsService;
    private AnalysisResult? _lastResult;
    private MixedFolderAnalysis? _lastMixedAnalysis;

    public PhotoSyncService(ISettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    private HashSet<string> GetRawExtensions() =>
        new(_settingsService.Current.RawExtensions, StringComparer.OrdinalIgnoreCase);

    public Task<AnalysisResult> AnalyzeFoldersAsync(
        string jpegFolderPath,
        string rawFolderPath,
        CancellationToken cancellationToken = default) => Task.Run(() =>
    {
        cancellationToken.ThrowIfCancellationRequested();

        var jpegFiles = Directory
            .EnumerateFiles(jpegFolderPath)
            .Where(f => JpegExtensions.Contains(Path.GetExtension(f)))
            .Select(f => new PhotoFile(f, Path.GetFileNameWithoutExtension(f), FileType.Jpeg))
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        var rawExts = GetRawExtensions();
        var rawFiles = Directory
            .EnumerateFiles(rawFolderPath)
            .Where(f => rawExts.Contains(Path.GetExtension(f)))
            .Select(f => new PhotoFile(f, Path.GetFileNameWithoutExtension(f), FileType.Raw))
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        var jpegBaseNames = new HashSet<string>(
            jpegFiles.Select(j => j.BaseName), StringComparer.OrdinalIgnoreCase);
        var rawBaseNames = new HashSet<string>(
            rawFiles.Select(r => r.BaseName), StringComparer.OrdinalIgnoreCase);

        var actions = new List<SyncAction>();

        foreach (var raw in rawFiles)
        {
            var actionType = jpegBaseNames.Contains(raw.BaseName)
                ? ActionType.KeepMatched
                : ActionType.DeleteRaw;
            actions.Add(new SyncAction(raw, actionType));
        }

        foreach (var jpeg in jpegFiles.Where(j => !rawBaseNames.Contains(j.BaseName)))
            actions.Add(new SyncAction(jpeg, ActionType.OrphanedJpeg));

        int matched = actions.Count(a => a.Action == ActionType.KeepMatched);
        int orphanedRaws = actions.Count(a => a.Action == ActionType.DeleteRaw);
        int orphanedJpegs = actions.Count(a => a.Action == ActionType.OrphanedJpeg);

        var stats = new SyncStatistics(
            TotalJpegs: jpegFiles.Count,
            TotalRaws: rawFiles.Count,
            MatchedPairs: matched,
            OrphanedRaws: orphanedRaws,
            OrphanedJpegs: orphanedJpegs);

        _lastResult = new AnalysisResult(stats, actions.AsReadOnly());
        return _lastResult;
    }, cancellationToken);

    public Task ExecuteCleanupAsync(CancellationToken cancellationToken = default)
    {
        if (_lastResult is null)
            throw new InvalidOperationException(
                "AnalyzeFoldersAsync must be called before ExecuteCleanupAsync.");

        return Task.Run(() =>
        {
            foreach (var action in _lastResult.Actions.Where(a => a.Action == ActionType.DeleteRaw))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (File.Exists(action.File.FullPath))
                    File.Delete(action.File.FullPath);
            }
        }, cancellationToken);
    }

    public async Task ExportReportAsync(
        string exportFilePath,
        CancellationToken cancellationToken = default)
    {
        if (_lastResult is null)
            throw new InvalidOperationException(
                "AnalyzeFoldersAsync must be called before ExportReportAsync.");

        var s = _lastResult.Statistics;
        var sb = new StringBuilder();

        sb.AppendLine("RawCleaner Report");
        sb.AppendLine($"Erstellt:,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("--- Statistik ---");
        sb.AppendLine($"Gesamt JPEGs,{s.TotalJpegs}");
        sb.AppendLine($"Gesamt RAWs,{s.TotalRaws}");
        sb.AppendLine($"Uebereinstimmungen,{s.MatchedPairs}");
        sb.AppendLine($"Verwaiste RAWs (werden geloescht),{s.OrphanedRaws}");
        sb.AppendLine($"Verwaiste JPEGs (behalten),{s.OrphanedJpegs}");
        sb.AppendLine();
        sb.AppendLine("--- Dateiaktionen ---");
        sb.AppendLine("Aktion,Dateiname,Vollstaendiger Pfad");

        foreach (var action in _lastResult.Actions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var label = action.Action switch
            {
                ActionType.KeepMatched => "Behalten (Match)",
                ActionType.DeleteRaw => "Geloescht (verwaist)",
                ActionType.OrphanedJpeg => "JPEG ohne RAW (behalten)",
                _ => action.Action.ToString()
            };
            sb.AppendLine($"{label},{action.File.BaseName},{action.File.FullPath}");
        }

        await File.WriteAllTextAsync(exportFilePath, sb.ToString(), Encoding.UTF8, cancellationToken);
    }

    public Task<MixedFolderAnalysis> AnalyzeMixedFolderAsync(
        string folderPath,
        CancellationToken cancellationToken = default) => Task.Run(() =>
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(folderPath))
            return MixedFolderAnalysis.Empty;

        var rawExts = GetRawExtensions();
        var jpegCount = 0;
        var rawFiles = new List<PhotoFile>();

        foreach (var path in Directory.EnumerateFiles(folderPath))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ext = Path.GetExtension(path);
            if (JpegExtensions.Contains(ext))
                jpegCount++;
            else if (rawExts.Contains(ext))
                rawFiles.Add(new PhotoFile(path, Path.GetFileNameWithoutExtension(path), FileType.Raw));
        }

        _lastMixedAnalysis = new MixedFolderAnalysis(jpegCount, rawFiles.Count, rawFiles.AsReadOnly());
        return _lastMixedAnalysis;
    }, cancellationToken);

    public Task ExecuteMoveAsync(
        string targetFolderPath,
        CancellationToken cancellationToken = default)
    {
        if (_lastMixedAnalysis is null)
            throw new InvalidOperationException(
                "AnalyzeMixedFolderAsync must be called before ExecuteMoveAsync.");

        return Task.Run(() =>
        {
            Directory.CreateDirectory(targetFolderPath);
            foreach (var raw in _lastMixedAnalysis.RawFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dest = Path.Combine(targetFolderPath, Path.GetFileName(raw.FullPath));
                if (File.Exists(raw.FullPath))
                    File.Move(raw.FullPath, dest, overwrite: false);
            }
        }, cancellationToken);
    }
}
