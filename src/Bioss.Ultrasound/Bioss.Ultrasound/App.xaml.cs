using Autofac;
using Bioss.Ultrasound.Ble;
using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.DI;
using Bioss.Ultrasound.Services;
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Sessions;
using Bioss.Ultrasound.UI.Pages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Bioss.Ultrasound
{
    public partial class App : Application
    {
        private readonly AppDatabase _database;
        private readonly ILogger _serverLogger;
        private readonly ISessionManager _sessionService;
        private readonly IUnsentLogDispatcher _unsentLogDispatcher;
        private readonly SessionCleanupService _sessionCleanup;
        public static Injector Injector { get; private set; }
        private static bool HasNetwork => Connectivity.NetworkAccess == NetworkAccess.Internet;
        public App()
        {
            InitializeComponent();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (Injector == null)
            {
                Injector = new Injector();
                Injector.RunWithMappedTypes(new Dictionary<Type, Type>());
            }

            if (HasNetwork)
                MainPage = new StartupPage();
            else
                MainPage = new NetworkUnvailable();



            _database = Injector.Container.Resolve<AppDatabase>();
            _serverLogger = Injector.Container.Resolve<ILogger>();
            _sessionService = Injector.Container.Resolve<ISessionManager>();
            _unsentLogDispatcher = Injector.Container.Resolve<IUnsentLogDispatcher>();
            _sessionCleanup = Injector.Container.Resolve<SessionCleanupService>();
        }

        protected override async void OnStart()
        {
            if (!HasNetwork)
                return;

            await _database.ConnectAsync();
            await _sessionService.StartSessionAsync();
                       
            var sendUnsentLogsTask =_unsentLogDispatcher.SendAllUnsentAsync();
            var removeOldSession = _sessionCleanup.RemoveOldSessionsAsync();
            await Task.WhenAny(sendUnsentLogsTask, removeOldSession);

            var devicesScaner = Injector.Container.Resolve<DevicesScaner>();
            devicesScaner.Start();

            // initialize hidden services
            var autoresetToco = Injector.Container.Resolve<AutoResetTocoService>();
            var appSettings = Injector.Container.Resolve<AppSettingsService>();

            autoresetToco.IsAutoResetToco = appSettings.IsAutoResetToco;
        }

        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            await _serverLogger.LogAsync(ex.Message, ServerLogLevel.FatalTerminationError);
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var access = e.NetworkAccess;
            if (access == NetworkAccess.Internet)   
                await _unsentLogDispatcher.SendAllUnsentAsync();
        }
    }
}
