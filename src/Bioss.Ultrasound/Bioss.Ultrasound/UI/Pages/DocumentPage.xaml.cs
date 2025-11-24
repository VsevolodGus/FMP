using Autofac;
using Bioss.Ultrasound.UI.ViewModels;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Pages
{
    public partial class DocumentPage : ContentPage
    {
        public DocumentPage()
        {
            InitializeComponent();
            BindingContext = App.Injector.Container.Resolve<DocumentViewModel>();
        }
    }
}
