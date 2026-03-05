using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.AppCompat.App;

namespace Bioss.Ultrasound.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        // отключает ночную тему
        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;
        base.OnCreate(savedInstanceState);
    }

    private static bool _permissionsRequested;
    protected override void OnResume()
    {
        base.OnResume();

        if (_permissionsRequested)
            return;

        _permissionsRequested = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                // ВАЖНО: Application = Microsoft.Maui.Controls.Application, не Android.App.Application
                if (Microsoft.Maui.Controls.Application.Current is App app)
                    await app.EnsureStartupPermissionsAsync();
            }
            catch
            {
                // при желании можно логировать
            }
        });
    }
}
