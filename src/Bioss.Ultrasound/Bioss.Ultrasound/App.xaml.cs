using Autofac;
using Bioss.Ultrasound.Ble;
using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.DI;
using Bioss.Ultrasound.Services;
using Bioss.Ultrasound.UI.Pages;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Bioss.Ultrasound
{
    public partial class App : Application
    {
        private readonly AppDatabase _database;
        public static Injector Injector { get; private set; }

        public App()
        {
            InitializeComponent();

            if (Injector == null)
            {
                Injector = new Injector();
                Injector.RunWithMappedTypes(new Dictionary<Type, Type>());
            }

            MainPage = new StartupPage();

            _database = Injector.Container.Resolve<AppDatabase>();
        }

        protected override async void OnStart()
        {
            await _database.ConnectAsync();

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
    }
}
