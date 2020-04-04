using Autofac;
using DataAccess.DbContext.MongoDB.Implementations;
using DataAccess.DbContext.MongoDB.Interfaces;

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
