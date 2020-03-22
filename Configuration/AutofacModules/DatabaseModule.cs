using Autofac;
using Config.Implementations;
using Config.Interfaces;
using DataAccess.DbContext.MongoDB.Implementations;
using DataAccess.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabase.Services.Articles.Implementations;
using GlobalArticleDatabase.Services.Articles.Interfaces;

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
