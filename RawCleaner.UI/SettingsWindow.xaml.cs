using RawCleaner.UI.ViewModels;
using Wpf.Ui.Controls;

namespace RawCleaner.UI;

public partial class SettingsWindow : FluentWindow
{
    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
