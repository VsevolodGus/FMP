using Autofac;
using Bioss.Ultrasound.UI.ViewModels;

namespace Bioss.Ultrasound.DI.Modules
{
    public class MvvmModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartupViewModel>();
            builder.RegisterType<MainViewModel>();
            builder.RegisterType<RecordsViewModel>();
            builder.RegisterType<RecordViewModel>();
            builder.RegisterType<MenuViewModel>();
            builder.RegisterType<SettingsViewModel>();
            builder.RegisterType<AboutViewModel>();
            builder.RegisterType<DocumentViewModel>();

            builder.RegisterType<BackupViewModel>();
        }
    }
}
