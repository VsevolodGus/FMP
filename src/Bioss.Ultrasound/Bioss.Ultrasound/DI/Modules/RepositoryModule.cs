using System.IO;
using Autofac;
using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Repository.Abstracts;
using Xamarin.Essentials;

namespace Bioss.Ultrasound.DI.Modules
{
    public class RepositoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "db.sqlite");
            var db = new AppDatabase(dbPath);

            builder.RegisterInstance(db).As<AppDatabase>().SingleInstance();
            builder.RegisterType<Repository.Repository>().As<IRepository>().SingleInstance();
        }
    }
}
