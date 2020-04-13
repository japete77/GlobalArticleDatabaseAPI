using DesiringGodArticlesCrawler.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace DesiringGodArticlesCrawler
{
    public class TopicsExtractor
    {
        public List<Topic> Extract(string url = "https://www.desiringgod.org/topics")
        {
            var results = new List<Topic>();

            HtmlWeb web = new HtmlWeb();
            var topicsHtml = web.Load(url);

            var topicColumns = topicsHtml.DocumentNode.SelectNodes("//div[@class='topic-column']");

            foreach (var column in topicColumns)
            {
                var topicColumnHtml = new HtmlDocument();
                topicColumnHtml.LoadHtml(column.InnerHtml);

                var topicNodes = topicColumnHtml.DocumentNode.SelectNodes("//ul[@class='link-list']/li/a");

                foreach (var topic in topicNodes)
                {
                    results.Add(new Topic
                    {
                        Name = System.Web.HttpUtility.HtmlDecode(topic.InnerText),
                        Link = $"https://www.desiringgod.org{topic.Attributes["href"].Value}"
                    });
                }
            }

            return results;
        }
    }
}
