using Autofac;
using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.DependencyExtensions;
using Bioss.Ultrasound.DI;
using Bioss.Ultrasound.Network;
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
        private readonly Task ConnectDb;

        public static Injector Injector { get; private set; }
      
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

            _database = Injector.Container.Resolve<AppDatabase>();
            _serverLogger = Injector.Container.Resolve<ILogger>();
            _sessionService = Injector.Container.Resolve<ISessionManager>();
            _unsentLogDispatcher = Injector.Container.Resolve<IUnsentLogDispatcher>();
            ConnectDb = _database.ConnectAsync();

            if (NetworkState.HasNetwork)
            {
                if (IsLastAndroidVersion)
                    MainPage = new MainTabbedPage();
                else
                    MainPage = new StartupPage(); 
            }
            else
                MainPage = new NetworkUnvailablePage();
        }

        protected override async void OnStart()
        {
            if (IsLastAndroidVersion)
                await CheckPermissionsAsync();

            if (!NetworkState.HasNetwork)
                return;

            await ConnectDb;
            await _sessionService.StartSessionAsync();
            await _unsentLogDispatcher.SendAllUnsentAsync();
            

            // initialize hidden services
            var autoresetToco = Injector.Container.Resolve<AutoResetTocoService>();
            var appSettings = Injector.Container.Resolve<AppSettingsService>();

            autoresetToco.IsAutoResetToco = appSettings.IsAutoResetToco;
        }

        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            await _serverLogger.LogAsync(ex.Message, ServerLogLevel.FatalTerminationError);
            await _sessionService.Exit();
        }

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            var access = e.NetworkAccess;
            if (NetworkState.AccessNetwork(access))   
                await _unsentLogDispatcher.SendAllUnsentAsync();
        }

        /// <summary>
        /// TODO отсюда выпилить, костыль 
        /// </summary>
        private bool IsLastAndroidVersion => DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 12;
        /// <summary>
        /// TODO тоже костыль, здесь не должно быть
        /// </summary>
        /// <returns></returns>
        private async Task<PermissionStatus> CheckPermissionsAsync()
        {
            var ble = DependencyService.Get<IPermission>();
            var status = await ble.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
                status = await ble.RequestAsync();

            return status;
        }
    }
}
