using System.Collections.Generic;

namespace GlobalArticleDatabaseAPI.Models
{
    public class ArticleSearchResponse
    {
        public long Total { get; set; }
        public long CurrentPage { get; set; }
        public List<Article> Articles { get; set; }
    }
}
