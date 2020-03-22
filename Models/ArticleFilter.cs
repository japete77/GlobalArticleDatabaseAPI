using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Models
{
    public class ArticleFilter
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Owner { get; set; }
        public string Language { get; set; }
        public bool? ByAuthorAsc { get; set; }
        public bool? ByDateAsc { get; set; }
        public bool? ByOwnerAsc { get; set; }
    }
}
