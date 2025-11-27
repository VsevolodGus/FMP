using Acr.UserDialogs;
using Bioss.Ultrasound.DependencyExtensions;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.UI.Pages;
using Libs.DI.ViewModels;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class StartupViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly IUserDialogs _dialogs;

        public StartupViewModel(ILogger logger, 
            IUserDialogs dialogs)
        {
            _logger = logger;
            _dialogs = dialogs;
        }

        public ICommand AppearingCommand => new Command(async a =>
        {
            if (await CheckPermissionsAsync() == PermissionStatus.Granted)
                NavigateToMain();
        });

        public ICommand RequestBluetoothPermissionCommand => new Command(async a =>
        {
            if (await CheckPermissionsAsync() == PermissionStatus.Granted)
                NavigateToMain();

            await PermissionSettingsDialog();
        });

        private void NavigateToMain()
        {
            App.Current.MainPage = new MainTabbedPage();
        }

        private async Task<PermissionStatus> CheckPermissionsAsync()
        {
            var ble = DependencyService.Get<IPermission>();
            var status = await ble.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await ble.RequestAsync();
                _logger.Log("Выдали устройству права");
            }

            return status;
        }

        private async Task PermissionSettingsDialog()
        {
            var title = string.Format(AppStrings.PermissionDialogTitle, "Bluetooth");
            var question = string.Format(AppStrings.PermissionDialogQuestion2, "Bluetooth");

            if (await _dialogs.ConfirmAsync(question, title, AppStrings.Settings, AppStrings.Cancel))
                AppInfo.ShowSettingsUI();
        }
    }
}
