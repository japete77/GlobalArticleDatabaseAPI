using Autofac;
using GlobalArticleDatabase.DbContext.MongoDB.Implementations;
using GlobalArticleDatabase.DbContext.MongoDB.Interfaces;

namespace GlobalArticleDatabase.Configuration.AutofacModules
{
    public class DatabaseModule : Module
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<DbContextMongoDb>().As<IDbContextMongoDb>().InstancePerLifetimeScope();
        }
    }
}
