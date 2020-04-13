using System;
using System.Collections.Generic;
using System.Text;

namespace DesiringGodArticlesCrawler.Models
{
    public class TranslateRequestV3
    {
        public string sourceLanguageCode { get; set; }
        public string targetLanguageCode { get; set; }
        public List<string> contents { get; set; }
    }
}
