using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using RawCleaner.Core.Helpers;
using RawCleaner.Core.Services;
using System.IO;
using Wpf.Ui.Controls;

namespace RawCleaner.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IPhotoSyncService _syncService;
    private readonly ISettingsService _settingsService;

    /// <summary>Injected by MainWindow to open the settings dialog without coupling VM to View.</summary>
    internal Action? ShowSettingsWindow { get; set; }

    public MainViewModel(IPhotoSyncService syncService, ISettingsService settingsService)
    {
        _syncService = syncService;
        _settingsService = settingsService;
        _isContextMenuRegistered = ContextMenuRegistrar.IsRegistered();
    }

    // -----------------------------------------------------------------------
    // Folder paths
    // -----------------------------------------------------------------------
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
    private string _jpegFolderPath = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
    private string _rawFolderPath = string.Empty;

    // -----------------------------------------------------------------------
    // Analysis results
    // -----------------------------------------------------------------------
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CleanupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportReportCommand))]
    [NotifyPropertyChangedFor(nameof(HasOrphanedRaws))]
    private bool _hasAnalysisResult;

    [ObservableProperty] private int _totalJpegs;
    [ObservableProperty] private int _totalRaws;
    [ObservableProperty] private int _matchedPairs;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CleanupCommand))]
    [NotifyPropertyChangedFor(nameof(HasOrphanedRaws))]
    private int _orphanedRaws;

    [ObservableProperty] private int _orphanedJpegs;

    /// <summary>True when analysis found orphaned RAWs - drives danger highlight in the UI.</summary>
    public bool HasOrphanedRaws => HasAnalysisResult && OrphanedRaws > 0;

    // -----------------------------------------------------------------------    // Move feature — mixed-folder sorting
    // -----------------------------------------------------------------------
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMixedContent))]
    [NotifyCanExecuteChangedFor(nameof(ExecuteMoveCommand))]
    private int _mixedFolderRawCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMixedContent))]
    [NotifyCanExecuteChangedFor(nameof(ExecuteMoveCommand))]
    private int _mixedFolderJpegCount;

    /// <summary>True when the JPEG folder contains both JPEG and RAW files.</summary>
    public bool HasMixedContent => MixedFolderRawCount > 0 && MixedFolderJpegCount > 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExecuteMoveCommand))]
    private string _moveTargetPath = string.Empty;

    // -----------------------------------------------------------------------    // UI state
    // -----------------------------------------------------------------------
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AnalyzeCommand))]
    [NotifyCanExecuteChangedFor(nameof(CleanupCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportReportCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string _statusMessage =
        "Bitte wählen Sie JPEG- und RAW-Ordner aus und starten Sie die Analyse.";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusSeverity))]
    private bool _isStatusError;

    public InfoBarSeverity StatusSeverity =>
        IsStatusError ? InfoBarSeverity.Error : InfoBarSeverity.Informational;

    // -----------------------------------------------------------------------
    // Context-menu toggle
    // -----------------------------------------------------------------------
    [ObservableProperty]
    private bool _isContextMenuRegistered;

    partial void OnIsContextMenuRegisteredChanged(bool oldValue, bool newValue)
    {
        try
        {
            var exe = Environment.ProcessPath ?? string.Empty;
            if (newValue)
                ContextMenuRegistrar.Register(exe);
            else
                ContextMenuRegistrar.Unregister();

            IsStatusError = false;
            StatusMessage = newValue
                ? "Kontextmenü-Eintrag wurde erfolgreich hinzugefügt."
                : "Kontextmenü-Eintrag wurde entfernt.";
        }
        catch (Exception ex)
        {
            IsStatusError = true;
            StatusMessage = $"Fehler beim Kontextmenü: {ex.Message}";
            // Directly reset the backing field to avoid retriggering this handler.
#pragma warning disable MVVMTK0034
            _isContextMenuRegistered = oldValue;
#pragma warning restore MVVMTK0034
            OnPropertyChanged(nameof(IsContextMenuRegistered));
        }
    }

    // -----------------------------------------------------------------------
    // Auto-detection helpers
    // -----------------------------------------------------------------------
    partial void OnJpegFolderPathChanged(string? oldValue, string newValue)
    {
        // Auto-detect RAW subfolder when not already filled
        if (!string.IsNullOrWhiteSpace(newValue) && string.IsNullOrWhiteSpace(RawFolderPath))
        {
            var detected = TryFindRawSubfolder(newValue);
            if (detected is not null)
                RawFolderPath = detected;
        }

        // Update move-target default
        MoveTargetPath = string.IsNullOrWhiteSpace(newValue)
            ? string.Empty
            : Path.Combine(newValue, _settingsService.Current.DefaultRawSubfolderName);

        // Reset and rescan for mixed content
        MixedFolderRawCount = 0;
        MixedFolderJpegCount = 0;
        if (!string.IsNullOrWhiteSpace(newValue))
            _ = CheckMixedContentAsync(newValue);
    }

    private string? TryFindRawSubfolder(string parentFolder)
    {
        foreach (var name in _settingsService.Current.RawFolderCandidates)
        {
            var candidate = Path.Combine(parentFolder, name);
            if (Directory.Exists(candidate))
                return candidate;
        }
        return null;
    }

    private async Task CheckMixedContentAsync(string folderPath)
    {
        try
        {
            var result = await _syncService.AnalyzeMixedFolderAsync(folderPath);
            MixedFolderRawCount = result.RawCount;
            MixedFolderJpegCount = result.JpegCount;
        }
        catch { /* silently ignore inaccessible folders */ }
    }

    // -----------------------------------------------------------------------
    // Commands – Folder browse
    // -----------------------------------------------------------------------
    [RelayCommand]
    private void BrowseJpegFolder()
    {
        var dlg = new OpenFolderDialog
        {
            Title = "JPEG-Ordner auswählen",
            InitialDirectory = string.IsNullOrWhiteSpace(JpegFolderPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                : JpegFolderPath
        };
        if (dlg.ShowDialog() == true)
            JpegFolderPath = dlg.FolderName;
    }

    [RelayCommand]
    private void BrowseRawFolder()
    {
        var dlg = new OpenFolderDialog
        {
            Title = "RAW-Ordner auswählen",
            InitialDirectory = string.IsNullOrWhiteSpace(RawFolderPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                : RawFolderPath
        };
        if (dlg.ShowDialog() == true)
            RawFolderPath = dlg.FolderName;
    }

    // -----------------------------------------------------------------------
    // Commands - Analyze
    // -----------------------------------------------------------------------
    private bool CanAnalyze() =>
        !IsBusy &&
        !string.IsNullOrWhiteSpace(JpegFolderPath) &&
        !string.IsNullOrWhiteSpace(RawFolderPath);

    [RelayCommand(CanExecute = nameof(CanAnalyze))]
    private async Task AnalyzeAsync()
    {
        IsBusy = true;
        IsStatusError = false;
        HasAnalysisResult = false;
        StatusMessage = "Analyse läuft...";

        try
        {
            var result = await _syncService.AnalyzeFoldersAsync(JpegFolderPath, RawFolderPath);
            var s = result.Statistics;

            TotalJpegs = s.TotalJpegs;
            TotalRaws = s.TotalRaws;
            MatchedPairs = s.MatchedPairs;
            OrphanedRaws = s.OrphanedRaws;
            OrphanedJpegs = s.OrphanedJpegs;

            HasAnalysisResult = true;
            StatusMessage =
                $"Analyse abgeschlossen - {s.OrphanedRaws} verwaiste RAW-Datei(en) gefunden.";
        }
        catch (Exception ex)
        {
            IsStatusError = true;
            StatusMessage = $"Fehler bei der Analyse: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // -----------------------------------------------------------------------
    // Commands - Cleanup
    // -----------------------------------------------------------------------
    private bool CanCleanup() => !IsBusy && HasAnalysisResult && OrphanedRaws > 0;

    [RelayCommand(CanExecute = nameof(CanCleanup))]
    private async Task CleanupAsync()
    {
        IsBusy = true;
        IsStatusError = false;
        StatusMessage = $"Bereinige {OrphanedRaws} verwaiste RAW-Datei(en) ...";

        try
        {
            await _syncService.ExecuteCleanupAsync();
            var deleted = OrphanedRaws;
            OrphanedRaws = 0;
            HasAnalysisResult = false;
            StatusMessage = $"Bereinigung abgeschlossen - {deleted} RAW-Datei(en) gelöscht.";
        }
        catch (Exception ex)
        {
            IsStatusError = true;
            StatusMessage = $"Fehler bei der Bereinigung: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // -----------------------------------------------------------------------
    // Commands - Export report
    // -----------------------------------------------------------------------
    private bool CanExportReport() => !IsBusy && HasAnalysisResult;

    [RelayCommand(CanExecute = nameof(CanExportReport))]
    private async Task ExportReportAsync()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Bericht exportieren",
            Filter = "CSV-Datei (*.csv)|*.csv|Alle Dateien (*.*)|*.*",
            FileName = $"RawCleaner_Bericht_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            DefaultExt = ".csv"
        };

        if (dlg.ShowDialog() != true) return;

        IsBusy = true;
        IsStatusError = false;
        StatusMessage = "Bericht wird exportiert ...";

        try
        {
            await _syncService.ExportReportAsync(dlg.FileName);
            StatusMessage = $"Bericht gespeichert: {dlg.FileName}";
        }
        catch (Exception ex)
        {
            IsStatusError = true;
            StatusMessage = $"Fehler beim Export: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // -----------------------------------------------------------------------
    // Commands – Move RAWs
    // -----------------------------------------------------------------------
    [RelayCommand]
    private void BrowseMoveTarget()
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Zielordner für RAW-Dateien auswählen",
            InitialDirectory = string.IsNullOrWhiteSpace(JpegFolderPath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                : JpegFolderPath
        };
        if (dlg.ShowDialog() == true)
            MoveTargetPath = dlg.FolderName;
    }

    private bool CanExecuteMove() =>
        !IsBusy && HasMixedContent && !string.IsNullOrWhiteSpace(MoveTargetPath);

    [RelayCommand(CanExecute = nameof(CanExecuteMove))]
    private async Task ExecuteMoveAsync()
    {
        IsBusy = true;
        IsStatusError = false;
        StatusMessage = $"Verschiebe {MixedFolderRawCount} RAW-Datei(en) nach \"{Path.GetFileName(MoveTargetPath)}\" ...";

        try
        {
            // Re-analyse to get the current file list before moving
            await _syncService.AnalyzeMixedFolderAsync(JpegFolderPath);
            await _syncService.ExecuteMoveAsync(MoveTargetPath);

            var moved = MixedFolderRawCount;
            var target = MoveTargetPath;

            MixedFolderRawCount = 0;
            MixedFolderJpegCount = 0;

            // Auto-fill RAW folder so the user can immediately run the sync analysis
            if (string.IsNullOrWhiteSpace(RawFolderPath))
                RawFolderPath = target;

            StatusMessage = $"{moved} RAW-Datei(en) erfolgreich nach \"{Path.GetFileName(target)}\" verschoben.";
        }
        catch (Exception ex)
        {
            IsStatusError = true;
            StatusMessage = $"Fehler beim Verschieben: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // -----------------------------------------------------------------------
    // Commands – Settings
    // -----------------------------------------------------------------------
    [RelayCommand]
    private void OpenSettings() => ShowSettingsWindow?.Invoke();
}
