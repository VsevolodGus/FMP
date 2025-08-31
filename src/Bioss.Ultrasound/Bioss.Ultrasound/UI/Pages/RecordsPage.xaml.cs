using Autofac;
using Bioss.Ultrasound.UI.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bioss.Ultrasound.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecordsPage : ContentPage
    {
        public RecordsPage()
        {
            InitializeComponent();
            BindingContext = App.Injector.Container.Resolve<RecordsViewModel>(new TypedParameter(typeof(INavigation), Navigation));
        }
    }
}