using DesiringGodArticlesCrawler.Models;
using HtmlAgilityPack;
using System.Collections.Generic;

namespace DesiringGodArticlesCrawler
{
    public class TopicsExtractor
    {
        public List<Topic> Extract(string url = "https://www.desiringgod.org/topics")
        {
            var results = new List<Topic>();

            HtmlWeb web = new HtmlWeb();
            var topicsHtml = web.Load(url);

            var topicColumnsLevel1 = topicsHtml.DocumentNode.SelectNodes("//div[@class='grouping-index topic-index']/ol/li/ol/li");
            var topicColumnsLevel2 = topicsHtml.DocumentNode.SelectNodes("//div[@class='grouping-index topic-index']/ol/li/ol/li/ol/li");

            foreach (var column in topicColumnsLevel1)
            {
                var topicColumnHtml = new HtmlDocument();
                topicColumnHtml.LoadHtml(column.InnerHtml);

                var topicNode = topicColumnHtml.DocumentNode.SelectSingleNode("a");
                var topicName = topicColumnHtml.DocumentNode.SelectSingleNode("a/div/h3");

                results.Add(new Topic
                {
                    Name = System.Web.HttpUtility.HtmlDecode(topicName.InnerText.Trim()),
                    Link = $"https://www.desiringgod.org{topicNode.Attributes["href"].Value.Trim()}"
                });
            }

            foreach (var column in topicColumnsLevel2)
            {
                var topicColumnHtml = new HtmlDocument();
                topicColumnHtml.LoadHtml(column.InnerHtml);

                var topicNode = topicColumnHtml.DocumentNode.SelectSingleNode("a");
                var topicName = topicColumnHtml.DocumentNode.SelectSingleNode("a/div/h3");

                results.Add(new Topic
                {
                    Name = System.Web.HttpUtility.HtmlDecode(topicName.InnerText.Trim()),
                    Link = $"https://www.desiringgod.org{topicNode.Attributes["href"].Value.Trim()}"
                });
            }

            return results;
        }
    }
}
