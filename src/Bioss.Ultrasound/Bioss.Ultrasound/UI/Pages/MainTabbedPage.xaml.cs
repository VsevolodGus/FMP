using Bioss.Ultrasound.Resources.Localization;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainTabbedPage : TabbedPage
    {
        private bool _isReplacingPage = false;

        public MainTabbedPage()
        {
            InitializeComponent();

            // Создаем только первую вкладку
            Children.Add(new NavigationPage(new MainPage())
            {
                IconImageSource = "ic_heart_pulse",
                Title = AppStrings.Main_Title
            });

            // Добавляем "заглушки" для остальных вкладок
            AddPlaceholderTab(AppStrings.Records_Title, "ic_records",
                () => new RecordsPage());
            AddPlaceholderTab(AppStrings.Menu_Title, "ic_menu",
                () => new MenuPage());
        }

        private void AddPlaceholderTab(string title, string icon, Func<Page> realPageFactory)
        {
            var placeholder = new ContentPage
            {
                Title = title,
                IconImageSource = icon,
                Content = new Label
                {
                    Text = AppStrings.Loading,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            };

            Children.Add(placeholder);

            placeholder.Appearing += async (sender, e) =>
            {
                if (sender is Page placeholderPage &&
                    placeholderPage.Parent is TabbedPage tabbedPage)
                {
                    var index = tabbedPage.Children.IndexOf(placeholderPage);
                    if (index >= 0 && tabbedPage.Children[index] == placeholderPage)
                    {
                        // Небольшая задержка чтобы UI успел отобразить "Загрузка..."
                        //await Task.Delay(10);

                        // Создаем реальную страницу в фоне
                        var realPage = new NavigationPage(realPageFactory())
                        {
                            Title = title,
                            IconImageSource = icon
                        };

                        // Ждем немного, чтобы страница инициализировалась
                        await Task.Delay(10);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            // Проверяем, что мы все еще на той же вкладке
                            if (tabbedPage.Children.Count > index &&
                                tabbedPage.Children[index] == placeholderPage)
                            {
                                // Временно запоминаем текущую страницу
                                var wasCurrent = tabbedPage.CurrentPage == placeholderPage;

                                // Заменяем
                                tabbedPage.Children[index] = realPage;

                                // Если эта вкладка была активной, делаем ее снова активной
                                if (wasCurrent)
                                {
                                    tabbedPage.CurrentPage = realPage;
                                }
                            }
                        });
                    }
                }
            };
        }
    }
}