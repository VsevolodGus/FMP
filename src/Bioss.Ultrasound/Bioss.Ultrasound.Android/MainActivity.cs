using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Acr.UserDialogs;
using Xamarin.Forms;
using Bioss.Ultrasound.DependencyExtensions;
using Bioss.Ultrasound.Droid.Extensions;
using System;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace Bioss.Ultrasound.Droid
{
    [Activity(
        Label = "@string/app_name",
        Icon = "@mipmap/ic_launcher", 
        Theme = "@style/SplashScreenTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize,
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetTheme(Resource.Style.MainTheme);
            // отключена темная тема
            AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;

            base.OnCreate(savedInstanceState);
            //  libs
            UserDialogs.Init(this);
            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();
            FormsMaterial.Init(this, savedInstanceState);
            Rg.Plugins.Popup.Popup.Init(this);

            //  Dependency
            DependencyService.Register<IPermission, BLEPermission>();
            DependencyService.Register<IPcmPlayer, PcmPlayer>();
            DependencyService.Register<ISystemVolume, SystemVolume>();

            //
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            AiForms.Renderers.Droid.SettingsViewInit.Init(); // need to write here
            LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private DateTime _lastPress;
        public override void OnBackPressed()
        {
            //  двойной клик для закрытия приложения
            if (AppHelper.IsRootPage)
            {
                var currentTime = DateTime.Now;
                var tt = currentTime - _lastPress;

                if (currentTime - _lastPress > TimeSpan.FromSeconds(5))
                {
                    Toast.MakeText(this, Resources.GetString(Resource.String.press_back_again), ToastLength.Long).Show();
                    _lastPress = currentTime;
                    return;
                }
            }

            Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed);
        }

        protected override async void OnDestroy()
        {
            // TODO вызвать закрытие сессии
            System.Diagnostics.Debug.WriteLine("MainActivity уничтожается.");
            

            base.OnDestroy();
        }
    }

    class AppHelper
    {
        public static bool IsRootPage
        {
            get
            {
                bool promptToConfirmExit = false;
                var mainPage = App.Current.MainPage;
                if (mainPage is FlyoutPage flayoutPage
                    && flayoutPage.Detail is NavigationPage detailNavigationPage)
                {
                    promptToConfirmExit = detailNavigationPage.Navigation.NavigationStack.Count <= 1;
                }
                else if (mainPage is NavigationPage navPage)
                {
                    if (navPage.CurrentPage is TabbedPage tabbedPage
                        && tabbedPage.CurrentPage is NavigationPage navigationPage)
                    {
                        promptToConfirmExit = navigationPage.Navigation.NavigationStack.Count <= 1;
                    }
                    else
                    {
                        promptToConfirmExit = mainPage.Navigation.NavigationStack.Count <= 1;
                    }
                }
                else if (mainPage is TabbedPage tabbedPage
                    && tabbedPage.CurrentPage is NavigationPage navigationPage)
                {
                    promptToConfirmExit = navigationPage.Navigation.NavigationStack.Count <= 1;
                }
                return promptToConfirmExit;
            }
        }
    }
}