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

        tabBar.Items.Add(CreateTab<MainPage>(services, AppStrings.Main_Title, "ic_heart_pulse.svg"));
        tabBar.Items.Add(CreateTab<RecordsPage>(services, AppStrings.Records_Title, "ic_records.svg"));
        tabBar.Items.Add(CreateTab<MenuPage>(services, AppStrings.Menu_Title, "ic_menu.svg"));

        Items.Add(tabBar);
    }

    private static ShellContent CreateTab<T>(IServiceProvider services, string title, string iconFile) where T : Page
    {
        return new ShellContent
        {
            Title = title,
            Icon = ImageSource.FromFile(iconFile),

            // ВАЖНО: фабрика создаёт страницу через DI, поэтому конструкторы с параметрами работают
            ContentTemplate = new DataTemplate(services.GetRequiredService<T>)
        };
    }
}
