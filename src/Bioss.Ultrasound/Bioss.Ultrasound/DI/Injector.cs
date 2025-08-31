using Autofac;
using Bioss.Ultrasound.DI.Modules;
using Libs.DI;
using Libs.DI.Factories;

namespace Bioss.Ultrasound.DI
{
    public class Injector : AutofacInjector
    {
        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            //  Регистрация модулей
            builder.RegisterModule<BleModule>();
            builder.RegisterModule<RepositoryModule>();
            builder.RegisterModule<ExtensionsModule>();
            builder.RegisterModule<DialogsModule>();
            builder.RegisterModule<MvvmModule>();
            builder.RegisterModule<ServicesModule>();
        }

        protected override void ConfigureApplication(IContainer container)
        {

        }

        protected override void RegisterViews(IViewFactory viewFactory)
        {

        }
    }
}
