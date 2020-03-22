using GlobalArticleDatabase.Configuration.AutofacModules;
using Autofac;

namespace GlobalArticleDatabase.Modules
{
    /// <summary>
    /// Default module for Autofac
    /// </summary>
    /// <remarks>
    /// See: https://github.com/drwatson1/AspNet-Core-REST-Service/wiki#dependency-injection
    /// </remarks>
    public class DefaultModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            ServicesModule.Register(builder);
            DatabaseModule.Register(builder);
        }
    }
}
