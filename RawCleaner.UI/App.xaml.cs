using System.IO;
using System.Windows;
using RawCleaner.Core.Services;
using RawCleaner.UI.ViewModels;

namespace RawCleaner.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settingsService = new SettingsService();
            var syncService = new PhotoSyncService(settingsService);
            var viewModel = new MainViewModel(syncService, settingsService);

            // If launched from Explorer context menu, pre-fill the JPEG folder.
            // OnJpegFolderPathChanged will auto-detect the RAW subfolder and
            // scan for mixed content automatically.
            if (e.Args.Length > 0 && Directory.Exists(e.Args[0]))
                viewModel.JpegFolderPath = e.Args[0];

            var window = new MainWindow(viewModel, settingsService);
            window.Show();
        }
    }
}
