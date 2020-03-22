using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Models
{
    public class ArticleSearchResponse
    {
        public long Total { get; set; }
        public long CurrentPage { get; set; }
        public List<Article> Articles { get; set; }
    }
}
