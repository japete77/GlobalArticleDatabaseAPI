using Autofac;
using Config.Implementations;
using Config.Interfaces;
using GlobalArticleDatabase.Services.Articles.Implementations;
using GlobalArticleDatabase.Services.Articles.Interfaces;
using GlobalArticleDatabaseAPI.Services.Articles.Implementations;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;

namespace GlobalArticleDatabase.Configuration.AutofacModules
{
    public class ServicesModule : Module
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<Settings>().As<ISettings>().InstancePerLifetimeScope();
            builder.RegisterType<ArticleService>().As<IArticleService>().InstancePerLifetimeScope();
            builder.RegisterType<TranslationService>().As<ITranslationService>().InstancePerLifetimeScope();
            builder.RegisterType<PublicationService>().As<IPublicationService>().InstancePerLifetimeScope();
            builder.RegisterType<S3Client>().As<IS3Client>().InstancePerLifetimeScope();
        }
    }
}
