using Autofac;
using Bioss.Ultrasound.UI.ViewModels;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Pages
{
    public partial class BackupPage : ContentPage
    {
        public BackupPage()
        {
            InitializeComponent();
            BindingContext = App.Injector.Container.Resolve<BackupViewModel>();
        }
    }
}
