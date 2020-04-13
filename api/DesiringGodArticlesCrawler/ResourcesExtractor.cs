using DesiringGodArticlesCrawler.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DesiringGodArticlesCrawler
{
    public class ResourcesExtractor
    {
        public List<Article> Extract(string url)
        {
            HtmlWeb web = new HtmlWeb();

            int page = 1;

            List<Article> articles = new List<Article>();

            while (true)
            {
                try
                {
                    var html = web.Load($"{url}?page={page}");

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html.Text);

                    var nodes = doc.DocumentNode.SelectNodes("//div[@class='card-list-view']/div[starts-with(@class,'card--resource')]");

                    if (nodes == null) break;

                    Console.WriteLine($"Processing page {page}...");

                    foreach (var node in nodes)
                    {
                        var articleHtml = new HtmlDocument();
                        articleHtml.LoadHtml(node.InnerHtml);

                        //var sss = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']");
                        //var ccc = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']/div[@class='card--resource__scripture']");
                        //var summary = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']")?.ChildNodes.Where(w => w.Name == "#text").FirstOrDefault()?.InnerText.Trim();
                        //var scripture = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']/div[@class='card--resource__scripture']")?.ChildNodes.Where(w => w.Name == "#text").FirstOrDefault()?.InnerText.Trim();

                        var link = articleHtml.DocumentNode.SelectSingleNode("//a[@class='card__shadow']")?.Attributes["href"]?.Value;

                        var date = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__date']")?.InnerText?.Trim();

                        articles.Add(
                            new Article
                            {
                                Category = articleHtml.DocumentNode.SelectSingleNode("//div[starts-with(@class, 'card--resource__labels-label')]")?.InnerText?.Trim(),
                                ImageLink = articleHtml.DocumentNode.SelectSingleNode("//a[@class='card__shadow']/div[@class='card__inner']/div/img")?.Attributes["data-src"]?.Value,
                                Link = $"https://www.desiringgod.org{link}",
                                Title = articleHtml.DocumentNode.SelectSingleNode("//h2[@class='card--resource__title']")?.InnerText?.Trim(),
                                Subtitle = articleHtml.DocumentNode.SelectSingleNode("//h3[@class='card--resource__subtitle']")?.InnerText?.Trim(),
                                Date = date != null ? DateTime.Parse(date) : new DateTime(),
                                Scripture = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']/div[@class='card--resource__scripture']")?.ChildNodes?.Where(w => w.Name=="#text").FirstOrDefault()?.InnerText?.Trim(),
                                Summary = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']")?.ChildNodes.Where(w => w.Name == "#text").FirstOrDefault()?.InnerText?.Trim()
                            }
                        );
                    }

                    page++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return articles;
        }
    }
}
