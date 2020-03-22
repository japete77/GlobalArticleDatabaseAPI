using Autofac;
using Config.Implementations;
using Config.Interfaces;
using GlobalArticleDatabase.Services.Articles.Implementations;
using GlobalArticleDatabase.Services.Articles.Interfaces;

namespace GlobalArticleDatabase.Configuration.AutofacModules
{
    public class ServicesModule : Module
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<Settings>().As<ISettings>().InstancePerLifetimeScope();
            builder.RegisterType<ArticleService>().As<IArticleService>().InstancePerLifetimeScope();
        }
    }
}
