using RawCleaner.Core.Services;
using RawCleaner.UI.ViewModels;
using Wpf.Ui.Controls;

namespace RawCleaner.UI
{
    public partial class MainWindow : FluentWindow
    {
        public MainWindow(MainViewModel viewModel, ISettingsService settingsService)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Wire up the settings window factory — creates a fresh VM on each open
            // so the dialog always reflects the currently persisted settings.
            viewModel.ShowSettingsWindow = () =>
            {
                var settingsVm = new SettingsViewModel(settingsService);
                var w = new SettingsWindow(settingsVm);
                w.Owner = this;
                settingsVm.SaveCompleted += w.Close;
                w.ShowDialog();
                settingsVm.SaveCompleted -= w.Close;
            };
        }
    }
}