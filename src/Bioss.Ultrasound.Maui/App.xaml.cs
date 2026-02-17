using Bioss.Ultrasound.Maui.Pages;

namespace Bioss.Ultrasound.Maui;

public partial class App : Application
{
    public App(IServiceProvider services)
    {
        InitializeComponent();

        MainPage = new NavigationPage(services.GetRequiredService<MenuPage>());

        //new AppShell();
    }
}
