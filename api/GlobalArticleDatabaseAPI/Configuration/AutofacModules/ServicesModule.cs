using Alintia.Glass.Api.Services.User.Implementations;
using Alintia.Glass.Services.User.Interfaces;
using Autofac;
using Config.Implementations;
using Config.Interfaces;
using GlobalArticleDatabaseAPI.DataAccess.Repositories.MongoDB;
using GlobalArticleDatabaseAPI.DbContext.MongoDB.Implementations;
using GlobalArticleDatabaseAPI.DbContext.MongoDB.Interfaces;
using GlobalArticleDatabaseAPI.Repositories.Interfaces;
using GlobalArticleDatabaseAPI.Services.Articles.Implementations;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
using GlobalArticleDatabaseAPI.Services.Authentication.Implementations;
using GlobalArticleDatabaseAPI.Services.Authentication.Interfaces;
using GlobalArticleDatabaseAPI.Services.ReferenceData.Implementations;
using GlobalArticleDatabaseAPI.Services.ReferenceData.Interfaces;
using GlobalArticleDatabaseAPI.Services.User.Implementations;
using GlobalArticleDatabaseAPI.Services.User.Interfaces;
using GlobalAtricleDatabase.Repositories.Implementations;

namespace GlobalArticleDatabaseAPI.Configuration.AutofacModules
{
    public class ServicesModule : Module
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<Settings>().As<ISettings>().InstancePerLifetimeScope();
            builder.RegisterType<ArticleService>().As<IArticleService>().InstancePerLifetimeScope();
            builder.RegisterType<TranslationService>().As<ITranslationService>().InstancePerLifetimeScope();
            builder.RegisterType<PublicationService>().As<IPublicationService>().InstancePerLifetimeScope();
            builder.RegisterType<ReferenceDataService>().As<IReferenceDataService>().InstancePerLifetimeScope();
            builder.RegisterType<S3Client>().As<IS3Client>().InstancePerLifetimeScope();

            builder.RegisterType<DbContextMongoDb>().As<IDbContextMongoDb>().InstancePerLifetimeScope();
            builder.RegisterType<AuthRenewRepositoryMongoDb>().As<IAuthRenewRepository>().InstancePerLifetimeScope();
            builder.RegisterType<UserRepositoryMongoDb>().As<IUserRepository>().InstancePerLifetimeScope();

            builder.RegisterType<JwtGenerator>().As<IJwtGenerator>().InstancePerLifetimeScope();
            builder.RegisterType<JwtRetriever>().As<IJwtRetriever>().InstancePerLifetimeScope();
            builder.RegisterType<RenewTokenCreator>().As<IRenewTokenCreator>().InstancePerLifetimeScope();
            builder.RegisterType<RenewTokenRemover>().As<IRenewTokenRemover>().InstancePerLifetimeScope();
            builder.RegisterType<RenewTokenRetriever>().As<IRenewTokenRetriever>().InstancePerLifetimeScope();
            builder.RegisterType<UserRetriever>().As<IUserRetriever>().InstancePerLifetimeScope();
            builder.RegisterType<UserUpdater>().As<IUserUpdater>().InstancePerLifetimeScope();
        }
    }
}
