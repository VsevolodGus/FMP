using Bioss.Ultrasound.Resources.Localization;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Bioss.Ultrasound.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainTabbedPage : TabbedPage
    {
        public MainTabbedPage()
        {
            InitializeComponent();

            Children.Add(new NavigationPage(new MainPage())
            {
                IconImageSource = "ic_heart_pulse",
                Title = AppStrings.Main_Title
            });

            Children.Add(new NavigationPage(new RecordsPage())
            {
                IconImageSource = "ic_records",
                Title = AppStrings.Records_Title
            });

            Children.Add(new NavigationPage(new MenuPage())
            {
                IconImageSource = "ic_menu",
                Title = AppStrings.Menu_Title
            });
        }
    }
}