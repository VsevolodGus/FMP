using System.Windows.Input;
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

        public string Version => "1.3.4";

        public ICommand PrivacyCommand => new Command(async a =>
        {
            await _navigation.PushModalAsync(new DocumentPage());
        });
    }
}
