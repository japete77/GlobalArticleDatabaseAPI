using Autofac;
using GlobalArticleDatabaseAPI.DbContext.MongoDB.Implementations;
using GlobalArticleDatabaseAPI.DbContext.MongoDB.Interfaces;

namespace GlobalArticleDatabaseAPI.Configuration.AutofacModules
{
    public class DatabaseModule : Module
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<DbContextMongoDb>().As<IDbContextMongoDb>().InstancePerLifetimeScope();
        }
    }
}
