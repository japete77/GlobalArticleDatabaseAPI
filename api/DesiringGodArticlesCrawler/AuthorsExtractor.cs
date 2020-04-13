using DesiringGodArticlesCrawler.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesiringGodArticlesCrawler
{
    public class AuthorsExtractor
    {
        public List<Author> Extract(string url = "https://www.desiringgod.org/authors")
        {
            var results = new List<Author>();

            HtmlWeb web = new HtmlWeb();
            var authorHtml = web.Load(url);

            var authorColumns = authorHtml.DocumentNode.SelectNodes("//div[@class='author']");

            foreach (var column in authorColumns)
            {
                var authorColumnHtml = new HtmlDocument();
                authorColumnHtml.LoadHtml(column.InnerHtml);

                var authorNode = authorColumnHtml.DocumentNode.SelectNodes("//h4/a");

                foreach (var author in authorNode)
                {
                    results.Add(new Author
                    {
                        Name = System.Web.HttpUtility.HtmlDecode(author.InnerText),
                        Link = $"https://www.desiringgod.org{author.Attributes["href"].Value}"
                    });
                }
            }

            return results;
        }
    }
}
