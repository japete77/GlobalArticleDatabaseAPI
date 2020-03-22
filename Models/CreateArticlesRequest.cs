using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Models
{
    public class CreateArticlesRequest
    {
        public List<CreateArticle> Articles { get; set; }
    }
}
