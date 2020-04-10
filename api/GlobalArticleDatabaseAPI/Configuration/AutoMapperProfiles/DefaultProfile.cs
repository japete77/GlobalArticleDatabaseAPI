using AutoMapper;
using GlobalArticleDatabaseAPI.DbContext.Models;
using GlobalArticleDatabaseAPI.Models;

namespace GlobalArticleDatabaseAPI.Configuration.AutoMapperProfiles
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<Article, ArticleEntity>().ReverseMap();
            CreateMap<Translation, TranslationEntity>().ReverseMap();
            CreateMap<Publication, PublicationEntity>().ReverseMap();
            CreateMap<User, UserEntity>().ReverseMap();
            CreateMap<Role, RoleEntity>().ReverseMap();
            CreateMap<AuthRenew, AuthRenewEntity>().ReverseMap();
        }
    }
}
