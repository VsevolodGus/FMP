using Autofac;
using Bioss.Ultrasound.Ble;
using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.DI;
using Bioss.Ultrasound.Services;
using Bioss.Ultrasound.Services.Network.Logging;
using Bioss.Ultrasound.Services.Network.Sessions;
using Bioss.Ultrasound.UI.Pages;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Bioss.Ultrasound
{
    public partial class App : Application
    {
        private readonly AppDatabase _database;
        private readonly ILogger _serverLogger;
        private readonly ISessionManager _sessionService;
        private readonly SessionCleanupService _sessionCleanup;
        public static Injector Injector { get; private set; }

        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (Injector == null)
            {
                Injector = new Injector();
                Injector.RunWithMappedTypes(new Dictionary<Type, Type>());
            }

            MainPage = new StartupPage();


            _database = Injector.Container.Resolve<AppDatabase>();
            _serverLogger = Injector.Container.Resolve<ILogger>();
            _sessionService = Injector.Container.Resolve<ISessionManager>();
            _sessionCleanup = Injector.Container.Resolve<SessionCleanupService>();
        }

        protected override async void OnStart()
        {
            await _database.ConnectAsync();
            await _sessionService.StartSessionAsync();
            //await _serverLogger.SendAllUnsentAsync();
            // TODO уточнить точно как тут должно работать
            // должно отработать фоном
            //await _sessionCleanup.RemoveOldSessionsAsync();

            var devicesScaner = Injector.Container.Resolve<DevicesScaner>();
            devicesScaner.Start();

            // initialize hidden services
            var autoresetToco = Injector.Container.Resolve<AutoResetTocoService>();
            var appSettings = Injector.Container.Resolve<AppSettingsService>();

            autoresetToco.IsAutoResetToco = appSettings.IsAutoResetToco;
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            await _serverLogger.LogAsync(ex.Message, ServerLogLevel.FatalTerminationError);
        }
    }
}
