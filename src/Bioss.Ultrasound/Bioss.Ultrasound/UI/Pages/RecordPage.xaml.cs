using Autofac;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.UI.ViewModels;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Pages
{
    public partial class RecordPage : ContentPage
    {
        public RecordPage(Record record)
        {
            InitializeComponent();
            BindingContext = App.Injector.Container.Resolve<RecordViewModel>(new TypedParameter(typeof(INavigation), Navigation), new TypedParameter(typeof(Record), record));
        }
    }
}
