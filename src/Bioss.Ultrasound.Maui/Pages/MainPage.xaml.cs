using Bioss.Ultrasound.Maui.ViewModels;

namespace Bioss.Ultrasound.Maui.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel mainViewModel)
    {
        InitializeComponent();
        BindingContext = mainViewModel;
    }
}