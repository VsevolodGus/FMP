using Autofac;
using Bioss.Ultrasound.Services;

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
        }
    }
}
