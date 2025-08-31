using Acr.UserDialogs;
using Autofac;

namespace Bioss.Ultrasound.DI.Modules
{
    public class DialogsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(UserDialogs.Instance).As<IUserDialogs>().SingleInstance();
        }
    }
}
