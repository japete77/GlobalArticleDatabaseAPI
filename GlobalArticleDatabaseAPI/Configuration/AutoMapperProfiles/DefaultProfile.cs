using AutoMapper;
using GlobalArticleDatabaseAPI.DbContext.Models;
using GlobalArticleDatabaseAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Configuration.AutoMapperProfiles
{
    public class DefaultProfile : Profile
    {
        public DefaultProfile()
        {
            CreateMap<Article, ArticleEntity>().ReverseMap();
            CreateMap<Translation, TranslationEntity>().ReverseMap();
            CreateMap<Publication, PublicationEntity>().ReverseMap();
        }
    }
}
