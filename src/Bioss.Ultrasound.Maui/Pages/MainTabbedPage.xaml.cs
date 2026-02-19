using Bioss.Ultrasound.Core.Resources.Localization;

namespace Bioss.Ultrasound.Maui.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class MainTabbedPage : TabbedPage
{
    public MainTabbedPage(RecordsPage recordsPage, MenuPage menuPage)
    {
        InitializeComponent();

        // 1) Главная вкладка (график/запись или что у тебя должно быть)
        Children.Add(new NavigationPage(menuPage)
        {
            IconImageSource = "ic_heart_pulse.png",
            Title = AppStrings.Main_Title
        });

        // 2) Записи
        Children.Add(new NavigationPage(recordsPage) // лучше тоже через DI, см. ниже
        {
            IconImageSource = "ic_records.png",
            Title = AppStrings.Records_Title
        });

        // 3) Меню
        Children.Add(new NavigationPage(menuPage)
        {
            IconImageSource = "ic_menu.png",
            Title = AppStrings.Menu_Title
        });
    }
}