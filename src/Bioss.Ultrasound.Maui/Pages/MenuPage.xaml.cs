using Bioss.Ultrasound.Maui.ViewModels;

namespace Bioss.Ultrasound.Maui.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MenuPage : ContentPage
{
    public MenuPage(MenuViewModel menuViewModel)
    {
        InitializeComponent();
        BindingContext = menuViewModel;
    }
}