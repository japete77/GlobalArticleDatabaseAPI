using DesiringGodArticlesCrawler.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace DesiringGodArticlesCrawler
{
    public class DGContentUpdater
    {
        private string cache_file = "articles_update.json";
        public async Task Update(DateTime from)
        {
            var allArticles = new List<Article>();

            if (!File.Exists(cache_file))
            {
                var articlesByDate = GetArticles("https://www.desiringgod.org/resources/all", from);

                var authors = articlesByDate.Select(s => s.Author.Trim()).Distinct().ToList();

                var articlesByAuthor = GetArticlesByAuthor(authors, from);

                var topicLinks = new TopicsExtractor().Extract();

                var articlesByTopic = GetArticlesByTopic(topicLinks, from);

                // Merge results                
                allArticles.AddRange(articlesByDate);

                // Aggregate by author summary
                foreach (var a in articlesByAuthor)
                {
                    var existing = allArticles.Where(w => w.Link == a.Link).FirstOrDefault();
                    if (existing != null)
                    {
                        // If exists, update the summary
                        existing.Summary = a.Summary;
                    }
                    else
                    {
                        allArticles.Add(a);
                    }
                }

                // Aggregate topics
                foreach (var byTopic in articlesByTopic)
                {
                    foreach (var a in byTopic.Value)
                    {
                        var existing = allArticles.Where(w => w.Link == a.Link).FirstOrDefault();
                        if (existing != null)
                        {
                            existing.Topics.Add(byTopic.Key);
                        }
                        else
                        {
                            a.Topics.Add(byTopic.Key);
                            allArticles.Add(a);
                        }
                    }
                }

                File.WriteAllText(cache_file, JsonConvert.SerializeObject(allArticles));
            }
            else
            {
                allArticles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(cache_file));
            }

            Console.WriteLine(">> Start creating articles...");

            await CreateArticles(allArticles);            
        }

        private async Task CreateArticles(List<Article> articles)
        {
            var articleExtractor = new ArticleExtractor();

            var client = await new ClientHelper().GetLoggedClient();

            foreach (var article in articles)
            {
                // Check if already exists in the database
                var searchUrl = $"articles?SourceLink={HttpUtility.UrlEncode(article.Link, Encoding.UTF8)}&page=1&pageSize=1";
                var responseGet = await client.GetAsync(searchUrl);
                if (responseGet.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var data = await responseGet.Content.ReadAsStringAsync();
                    var dbArticles = JsonConvert.DeserializeObject<GlobalArticleDatabaseAPI.Models.ArticleSearchResponse>(data);

                    if (dbArticles.Total == 0)
                    {
                        string content = TextToHtml(articleExtractor.Extract(article.Link));

                        // Create the article...                                
                        var bodyArticle = new GlobalArticleDatabaseAPI.Models.CreateArticleRequest
                        {
                            Article = new GlobalArticleDatabaseAPI.Models.Article
                            {
                                Author = article.Author,
                                Category = article.Category,
                                Date = article.Date,
                                HasText = true,
                                SourceLink = article.Link,
                                ImageLink = article.ImageLink,
                                Language = "us",
                                Owner = "Desiring God",
                                Title = article.Title,
                                Subtitle = article.Subtitle,
                                Summary = article.Summary,
                                Reference = article.Scripture,
                                Topics = article.Topics,
                                Characters = Program.TextLength(article.Title) +
                                   Program.TextLength(article.Subtitle) +
                                   Program.TextLength(article.Summary) +
                                   Program.TextLength(content),
                                Words = Program.WordsCount(article.Title) +
                                    Program.WordsCount(article.Subtitle) +
                                    Program.WordsCount(article.Summary) +
                                    Program.WordsCount(content)
                            },
                            Text = content,
                        };

                        var postContent = new StringContent(JsonConvert.SerializeObject(bodyArticle), Encoding.UTF8, "application/json");

                        var responseArticle = await client.PostAsync("article", postContent);

                        if (responseArticle.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var createdData = await responseArticle.Content.ReadAsStringAsync();
                            var newArticle = JsonConvert.DeserializeObject<GlobalArticleDatabaseAPI.Models.CreateArticleResponse>(createdData);

                            Console.WriteLine($"Added article {article.Link}");
                        }
                        else
                        {
                            Console.WriteLine($"Error {responseArticle.StatusCode} creating article {article.Link}");
                        }
                    }
                    else
                    {
                        // Finish the update...
                        Console.WriteLine($"Found article {article.Link}");                        
                    }
                }
                else
                {
                    Console.WriteLine($"Error calling service {responseGet.ReasonPhrase}");
                    return;
                }
            }
        }

        private string TextToHtml(string text)
        {
            if (text != null)
            {
                var m = Regex.Match(text, @"<[^>]*?>");
                
                if (!m.Success)
                {
                    var paragraphs = Regex.Split(text, @"(\r\n?|\n){2}")
                          .Where(p => p.Any(char.IsLetterOrDigit));

                    StringBuilder sb = new StringBuilder();
                    foreach (var paragraph in paragraphs)
                    {
                        sb.AppendLine($"<p>{paragraph}</p>");
                    }
                    return sb.ToString();
                }

                return text;
            }

            return null;
        }

        private Dictionary<string, List<Article>> GetArticlesByTopic(List<Topic> topics, DateTime from)
        {
            var result = new Dictionary<string, List<Article>>();
            
            foreach (var topic in topics)
            {
                Console.WriteLine($"Retrieving articles from {topic.Link}");

                var articles = GetArticles($"{topic.Link}/all", from);
                if (articles.Count > 0)
                {
                    result.Add(topic.Name, articles);
                }
            }

            return result;
        }

        private List<Author> GetAuthorLinks(List<string> authors)
        {
            var authorLinks = new List<Author>();

            HtmlWeb web = new HtmlWeb();

            var authorHtml = web.Load("https://www.desiringgod.org/authors");

            var authorColumns = authorHtml.DocumentNode.SelectNodes("//div[@class='author']");

            foreach (var column in authorColumns)
            {
                var authorColumnHtml = new HtmlDocument();
                authorColumnHtml.LoadHtml(column.InnerHtml);

                var authorNode = authorColumnHtml.DocumentNode.SelectNodes("//h4/a");

                foreach (var author in authorNode)
                {
                    var name = System.Web.HttpUtility.HtmlDecode(author.InnerText).Trim();
                    if (authors.Contains(name))
                    {
                        authorLinks.Add(new Author
                        {
                            Name = name,
                            Link = $"https://www.desiringgod.org{author.Attributes["href"].Value}"
                        });
                    }
                }
            }

            return authorLinks;
        }

        private List<Article> GetArticlesByAuthor(List<string> authors, DateTime from)
        {
            List<Article> results = new List<Article>();

            var authorLinks = GetAuthorLinks(authors);

            foreach (var authorLink in authorLinks)
            {
                results.AddRange(GetArticles(authorLink.Link, from));
            }

            return results;
        }

        private List<Article> GetArticles(string url, DateTime from)
        {
            List<Article> results = new List<Article>();

            HtmlWeb web = new HtmlWeb();

            int page = 1;

            while (true)
            {
                var html = web.Load($"{url}?page={page}");

                var doc = new HtmlDocument();
                doc.LoadHtml(html.Text);

                var nodes = doc.DocumentNode.SelectNodes("//div[@class='card-list-view']/div[starts-with(@class,'card--resource')]");

                if (nodes == null) return results;

                Console.WriteLine($"Processing page {page}...");

                foreach (var node in nodes)
                {
                    var articleHtml = new HtmlDocument();

                    articleHtml.LoadHtml(node.InnerHtml);

                    var link = articleHtml.DocumentNode.SelectSingleNode("//a[@class='card__shadow']")?.Attributes["href"]?.Value;

                    var date = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__date']")?.InnerText?.Trim();

                    var article =
                        new Article
                        {
                            Author = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card__author']")?.InnerText?.Trim(),
                            Category = articleHtml.DocumentNode.SelectSingleNode("//div[starts-with(@class, 'card--resource__labels-label')]")?.InnerText?.Trim(),
                            ImageLink = HttpUtility.HtmlDecode(articleHtml.DocumentNode.SelectSingleNode("//a[@class='card__shadow']/div[@class='card__inner']/div/img")?.Attributes["data-src"]?.Value),
                            Link = $"https://www.desiringgod.org{link}",
                            Title = articleHtml.DocumentNode.SelectSingleNode("//h2[@class='card--resource__title']")?.InnerText?.Trim(),
                            Subtitle = articleHtml.DocumentNode.SelectSingleNode("//h3[@class='card--resource__subtitle']")?.InnerText?.Trim(),
                            Date = date != null ? DateTime.Parse(date) : new DateTime(),
                            Scripture = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']/div[@class='card--resource__scripture']")?.ChildNodes?.Where(w => w.Name == "#text").FirstOrDefault()?.InnerText?.Trim(),
                            Summary = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']")?.ChildNodes.Where(w => w.Name == "#text").FirstOrDefault()?.InnerText?.Trim(),
                            Topics = new List<string>()
                        };

                    if (DateTime.Compare(article.Date, from) < 0) return results;

                    results.Add(article);

                    Console.WriteLine($"Retrieved article {article.Date.ToShortDateString()} {article.Link}");
                }

                page++;
            }
        }
    }
}
