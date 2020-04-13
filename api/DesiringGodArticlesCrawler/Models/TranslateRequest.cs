using System;
using System.Collections.Generic;
using System.Text;

namespace DesiringGodArticlesCrawler.Models
{
    public class TranslateRequest
    {
        public List<string> q { get; set; }
        public string target { get; set; }
    }
}
