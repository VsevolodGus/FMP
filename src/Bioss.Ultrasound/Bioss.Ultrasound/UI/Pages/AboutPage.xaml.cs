using Autofac;
using Bioss.Ultrasound.UI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bioss.Ultrasound.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            BindingContext = App.Injector.Container.Resolve<AboutViewModel>(new TypedParameter(typeof(INavigation), Navigation));
        }
    }
}