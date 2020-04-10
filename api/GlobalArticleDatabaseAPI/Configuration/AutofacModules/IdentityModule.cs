using Autofac;
using GlobalArticleDatabase.Identity.Implementation;
using GlobalArticleDatabaseAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace GlobalArticleDatabase.Configuration.AutofacModules
{
    public class IdentityModule : Module
    {
        public static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<UserStore>().As<IUserStore<User>>().InstancePerLifetimeScope();
        }
    }
}
