using System.Windows.Input;
using Bioss.Ultrasound.Resources.Localization;
using Bioss.Ultrasound.UI.Pages;
using Libs.DI.ViewModels;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private readonly INavigation _navigation;

        public AboutViewModel(INavigation navigation)
        {
            _navigation = navigation;
        }

        public string Version => "1.0.30";

        public ICommand PrivacyCommand => new Command(async a =>
        {
            await _navigation.PushAsync(new DocumentPage(AppStrings.DocPrivacy));
        });
    }
}
