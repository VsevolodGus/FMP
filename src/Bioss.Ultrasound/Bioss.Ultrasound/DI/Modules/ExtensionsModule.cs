using Autofac;
using Bioss.Ultrasound.DependencyExtensions;
using Xamarin.Forms;

namespace Bioss.Ultrasound.DI.Modules
{
    public class ExtensionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var player = DependencyService.Get<IPcmPlayer>();
            builder.RegisterInstance(player).As<IPcmPlayer>().SingleInstance();

            var volume = DependencyService.Get<ISystemVolume>();
            builder.RegisterInstance(volume).As<ISystemVolume>().SingleInstance();
        }
    }
}
