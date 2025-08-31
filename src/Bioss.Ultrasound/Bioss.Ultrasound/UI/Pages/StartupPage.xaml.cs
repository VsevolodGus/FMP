using Autofac;
using Bioss.Ultrasound.UI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bioss.Ultrasound.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StartupPage : ContentPage
    {
        public StartupPage()
        {
            InitializeComponent();

            BindingContext = App.Injector.Container.Resolve<StartupViewModel>();
        }
    }
}