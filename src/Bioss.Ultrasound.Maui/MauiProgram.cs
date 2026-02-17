using Bioss.Ultrasound.Maui.Navigation;
using Bioss.Ultrasound.Maui.Pages;
using Bioss.Ultrasound.Maui.ViewModels;

namespace Bioss.Ultrasound.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // TODO вынести куда либо
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        builder.Services.AddTransient<AboutPage>();
        builder.Services.AddTransient<DocumentPage>();
        builder.Services.AddTransient<MenuPage>();

        builder.Services.AddTransient<AboutViewModel>();
        builder.Services.AddTransient<DocumentViewModel>();
        builder.Services.AddTransient<MenuViewModel>();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		//builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
