using Bioss.Ultrasound.UI.ViewModels;
using Xamarin.Forms;

namespace Bioss.Ultrasound.UI.Pages
{
    public partial class DocumentPage : ContentPage
    {
        public DocumentPage(string documentName)
        {
            InitializeComponent();
            BindingContext = new DocumentViewModel(documentName);
        }
    }
}
