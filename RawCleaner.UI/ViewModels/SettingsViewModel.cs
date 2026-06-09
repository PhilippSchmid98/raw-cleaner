using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RawCleaner.Core.Models;
using RawCleaner.Core.Services;

namespace RawCleaner.UI.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    /// <summary>Raised by <see cref="SaveCommand"/> so the View can close the window.</summary>
    public event Action? SaveCompleted;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        LoadFromSettings(settingsService.Current);
    }

    [ObservableProperty]
    private string _rawExtensionsText = string.Empty;

    [ObservableProperty]
    private string _rawFolderCandidatesText = string.Empty;

    [ObservableProperty]
    private string _defaultRawSubfolderName = string.Empty;

    [RelayCommand]
    private void Save()
    {
        var settings = new AppSettings
        {
            RawExtensions = ParseList(RawExtensionsText),
            RawFolderCandidates = ParseList(RawFolderCandidatesText),
            DefaultRawSubfolderName = DefaultRawSubfolderName.Trim()
        };
        _settingsService.Save(settings);
        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private void Reset()
    {
        _settingsService.Reset();
        LoadFromSettings(_settingsService.Current);
    }

    private void LoadFromSettings(AppSettings settings)
    {
        RawExtensionsText = string.Join(", ", settings.RawExtensions);
        RawFolderCandidatesText = string.Join(", ", settings.RawFolderCandidates);
        DefaultRawSubfolderName = settings.DefaultRawSubfolderName;
    }

    private static List<string> ParseList(string text) =>
        text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
}
