using Autofac;
using Bioss.Ultrasound.Services;
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Server;
using Bioss.Ultrasound.Services.Sessions;

namespace Bioss.Ultrasound.DI.Modules
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppSettingsService>().SingleInstance();
            builder.RegisterType<InfoSettingsService>().SingleInstance();
            builder.RegisterType<AutoResetTocoService>().SingleInstance();
            builder.RegisterType<AudioService>().SingleInstance();


            builder.RegisterType<SessionManager>().As<ISessionManager>().SingleInstance();
            builder.RegisterType<SessionCleanupService>().SingleInstance();
            builder.RegisterType<ServerHttpProvider>().SingleInstance();
            builder.RegisterType<ServerLogger>()
                .As<ILogger>()
                .As<IUnsentLogDispatcher>()
                .SingleInstance();
        }
    }
}
