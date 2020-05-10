using System;
using System.Collections.Generic;

namespace DesiringGodArticlesCrawler.Models
{
    public class Article
    {
        public List<string> Author { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Link { get; set; }
        public string ImageLink { get; set; }
        public DateTime Date { get; set; }
        public string Summary { get; set; }
        public string Scripture { get; set; }
        public List<string> Topics { get; set; }
        public string Text { get; set; }
    }

    public class ArticleOld
    {
        public string Author { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Link { get; set; }
        public string ImageLink { get; set; }
        public DateTime Date { get; set; }
        public string Summary { get; set; }
        public string Scripture { get; set; }
        public List<string> Topics { get; set; }
        public string Text { get; set; }
    }
}
