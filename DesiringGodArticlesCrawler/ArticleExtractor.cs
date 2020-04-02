using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DesiringGodArticlesCrawler;
using HtmlAgilityPack;

namespace DesiringGodArticlesCrawler
{
    public class ArticleExtractor
    {
        public string Extract(string url)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);

            HtmlWeb web = new HtmlWeb();
            var articleHtml = web.Load(url);

            var body = articleHtml.DocumentNode.SelectSingleNode("//div[contains(@class,'resource__body')]");

            List<string> articleParagraphs = new List<string>();
            foreach (var node in body.ChildNodes)
            {
                if (node?.NextSibling?.Name == "p" ||
                    node?.NextSibling?.Name == "h1" ||
                    node?.NextSibling?.Name == "h2" ||
                    node?.NextSibling?.Name == "h3" ||
                    node?.NextSibling?.Name == "h4" ||
                    node?.NextSibling?.Name == "pre" ||
                    node?.NextSibling?.Name == "ul" ||
                    node?.NextSibling?.Name == "ol" ||
                    (node?.NextSibling?.Name == "blockquote" && node?.NextSibling?.Attributes?.Count == 0) ||
                    (node?.NextSibling?.Name == "blockquote" && node?.NextSibling?.Attributes?["class"] != null && node?.NextSibling?.Attributes?["class"].Value == "quotes"))
                {
                    var text = node.NextSibling.InnerText.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        text = regex.Replace(text, " ");
                        articleParagraphs.Add(System.Web.HttpUtility.HtmlDecode(text));
                    }
                }
            }

            StringBuilder sb = new StringBuilder();

            articleParagraphs.ForEach(line => {
                sb.AppendLine(line);
                sb.AppendLine();
            });

            return sb.ToString();
        }
    }
}
