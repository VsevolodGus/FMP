using Bioss.Ultrasound.DependencyExtensions;
using Bioss.Ultrasound.iOS.Extensions;
using Bioss.Ultrasound.Services.Sessions;
using Foundation;
using UIKit;
using Xamarin.Forms;

namespace Bioss.Ultrasound.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();
            FormsMaterial.Init();
            Rg.Plugins.Popup.Popup.Init();

            //  Dependency
            DependencyService.Register<IPermission, BLEPermission>();
            DependencyService.Register<IPcmPlayer, PcmPlayer>();
            DependencyService.Register<ISystemVolume, SystemVolume>();

            NSNotificationCenter.DefaultCenter.AddObserver(
                UIApplication.WillTerminateNotification,
                async notification => 
                {
                    try
                    {
                        var sessionManager = DependencyService.Resolve<ISessionManager>();
                        await sessionManager.Exit();
                    }
                    catch
                    {
                        System.Diagnostics.Debug.WriteLine("Ошибка при закрытии сессии");
                    }
                });


            global::Xamarin.Forms.Forms.Init();
            AiForms.Renderers.iOS.SettingsViewInit.Init(); // need to write there
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}
