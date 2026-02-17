using Bioss.Ultrasound.Maui.ViewModels;

namespace Bioss.Ultrasound.Maui.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class RecordsPage : ContentPage
{
    public RecordsPage(RecordsViewModel recordsViewModel)
    {
        InitializeComponent();
        BindingContext = recordsViewModel;
    }
}