using Bioss.Ultrasound.Maui.ViewModels;

namespace Bioss.Ultrasound.Maui.Pages;

public partial class AboutPage : ContentPage
{
    public AboutPage(AboutViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
