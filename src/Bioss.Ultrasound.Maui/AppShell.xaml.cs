using Bioss.Ultrasound.Core.Resources.Localization;
using Bioss.Ultrasound.Maui.Pages;

namespace Bioss.Ultrasound.Maui;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();

        // TabBar снизу
        var tabBar = new TabBar();

        tabBar.Items.Add(CreateTab(services, AppStrings.Main_Title, "ic_heart_pulse.png", typeof(MenuPage)));
        tabBar.Items.Add(CreateTab(services, AppStrings.Records_Title, "ic_records.png", typeof(RecordsPage)));
        tabBar.Items.Add(CreateTab(services, AppStrings.Menu_Title, "ic_menu.png", typeof(MenuPage)));

        Items.Add(tabBar);
    }

    private static ShellContent CreateTab(IServiceProvider services, string title, string icon, Type pageType)
    {
        return new ShellContent
        {
            Title = title,
            Icon = icon,

            // ВАЖНО: фабрика создаёт страницу через DI, поэтому конструкторы с параметрами работают
            ContentTemplate = new DataTemplate(() => (Page)services.GetRequiredService(pageType))
        };
    }
}
