using DesiringGodArticlesCrawler.Models;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DesiringGodArticlesCrawler
{
    internal class AuthorsResponse
    {
        public List<string> Items { get; set; }
    }
    public class IXMarksContentUpdater
    {
        private List<string> categories = new List<string>
        {
            "Article",
            "Answer",
            "Interview",
            "Mailbag",
            "Message",
            "Review"
        };

        private async Task FixIssues()
        {
            var client = await new ClientHelper().GetLoggedClient(password: "Fou0rier!");
            var httpResponseAuthors = await client.GetAsync("reference-data/authors");
            if (httpResponseAuthors.StatusCode != System.Net.HttpStatusCode.OK) return;
            var str = await httpResponseAuthors.Content.ReadAsStringAsync();
            var listAuthors = JsonConvert.DeserializeObject<AuthorsResponse>(str);

            // Authors map...
            Dictionary<string, string> authorsMap = new Dictionary<string, string>();
            listAuthors.Items.ForEach(item => authorsMap.Add(item, item));

            // Leemos los autores de la lista...
            var articles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText("9marks_all.json"));

            var authors = articles.SelectMany(s => s.Author).Distinct().OrderBy(o => o).ToList();
            
            //// Vamos a mapear la nueva lista de artículos con soporte para multiple autores
            //List<Article> newArticles = new List<Article>();

            //articles.ForEach(item =>
            //{
            //    var authors = new List<string>(item.Author.Split(","));
            //    authors.ForEach(i => i = i.Trim());

            //    newArticles.Add(new Article
            //    {
            //        Author = authors,
            //        Category = item.Category,
            //        Date = item.Date,
            //        ImageLink = item.ImageLink,
            //        Link = item.Link,
            //        Scripture = item.Scripture,
            //        Subtitle = item.Subtitle,
            //        Summary = item.Summary,
            //        Text = item.Text,
            //        Title = item.Title,
            //        Topics = item.Topics
            //    });
            //});

            var ixAuthors = articles.SelectMany(s => s.Author).Distinct().ToList();

            int articleCount = 0;

            for (int i = 0; i < articles.Count; i++)
            {
                for (int j = 0; j < articles[i].Author.Count; j++)
                {
                    articles[i].Author[j] = articles[i].Author[j].Trim();

                    if (articles[i].Author[j].Contains("."))
                    {
                        if (authorsMap.ContainsKey(articles[i].Author[j]))
                        {
                            Console.WriteLine($"Article #{articleCount}/{articles.Count}: Mapping {articles[i].Author[j]} to {authorsMap[articles[i].Author[j]]}");
                            articles[i].Author[j] = authorsMap[articles[i].Author[j]];
                        }
                        else
                        {
                            var names = articles[i].Author[j].Split(" ");
                            var lastName = names[names.Length - 1];

                            var match = ixAuthors.Where(w => !w.Contains(".") && w.Split(" ").Any(a => a == lastName)).ToList();
                            var match2 = listAuthors.Items.Where(w => w.Split(" ").Any(a => a == lastName)).ToList();

                            var finalMatch = new List<string>();
                            finalMatch.AddRange(match);
                            finalMatch.AddRange(match2);

                            if (finalMatch.Count > 0)
                            {
                                Console.WriteLine();
                                Console.WriteLine("-------------------------------------------------------------------");
                                Console.WriteLine($"Article #{articleCount}/{articles.Count}: Match for <{articles[i].Author[j]}>:");
                                finalMatch.ForEach(m => Console.WriteLine($" > {m}"));
                                Console.WriteLine($"Type the author name or Enter to select default <{finalMatch.Last()}>");
                                var response = Console.ReadLine();
                                if (string.IsNullOrEmpty(response))
                                {
                                    authorsMap.Add(articles[i].Author[j], finalMatch.Last());
                                    articles[i].Author[j] = finalMatch.Last();
                                }
                                else
                                {
                                    authorsMap.Add(articles[i].Author[j], response);
                                    articles[i].Author[j] = response;
                                }
                            }
                        }
                    }
                }
            }

            // Normalize author names using author map
            //articles.ForEach(article =>
            //{
            //    article.Author.ForEach(author =>
            //    {
            //        author = author.Trim();

            //        if (author.Contains("."))
            //        {
            //            if (authorsMap.ContainsKey(author))
            //            {
            //                Console.WriteLine($"Article #{articleCount}/{articles.Count}: Mapping {author} to {authorsMap[author]}");
            //                author = authorsMap[author];
            //            }
            //            else
            //            {
            //                var names = author.Split(" ");
            //                var lastName = names[names.Length - 1];

            //                var match = ixAuthors.Where(w => !w.Contains(".") && w.Split(" ").Any(a => a == lastName)).ToList();
            //                var match2 = listAuthors.Items.Where(w => w.Split(" ").Any(a => a == lastName)).ToList();

            //                var finalMatch = new List<string>();
            //                finalMatch.AddRange(match);
            //                finalMatch.AddRange(match2);

            //                if (finalMatch.Count > 0)
            //                {
            //                    Console.WriteLine();
            //                    Console.WriteLine("-------------------------------------------------------------------");
            //                    Console.WriteLine($"Article #{articleCount}/{articles.Count}: Match for <{author}>:");
            //                    finalMatch.ForEach(m => Console.WriteLine($" > {m}"));
            //                    Console.WriteLine($"Type the author name or Enter to select default <{finalMatch.Last()}>");
            //                    var response = Console.ReadLine();
            //                    if (string.IsNullOrEmpty(response))
            //                    {
            //                        authorsMap.Add(author, finalMatch.Last());
            //                        author = finalMatch.Last();
            //                    }
            //                    else
            //                    {
            //                        authorsMap.Add(author, response);
            //                        author = response;
            //                    }
            //                }
            //            }
            //        }
            //    });

            //    articleCount++;
            // });

            File.WriteAllText("authors_map.json", JsonConvert.SerializeObject(authorsMap));

            File.WriteAllText("9marks_all.json", JsonConvert.SerializeObject(articles));
        }

        public async Task Update(DateTime from)
        {
            var allArticles = new List<Article>();

            if (File.Exists("9marks_all.json"))
            {
                allArticles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText("9marks_all.json"));
            }
            else
            {
                foreach (var category in categories)
                {
                    var articlesFile = $"9marks_{category.ToLower()}.json";

                    Console.WriteLine($">>> Processing {category}");

                    if (!File.Exists(articlesFile))
                    {
                        // Retrieve articles
                        allArticles = await GetContentByCategory(category, from);
                        File.WriteAllText(articlesFile, JsonConvert.SerializeObject(allArticles));
                    }
                    else
                    {
                        var articles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(articlesFile));
                        allArticles.AddRange(articles);
                    }

                    Console.WriteLine($">>> Retrieved {allArticles.Count} articles");
                }
            }

            // Update in GADB
            await CreateArticles(allArticles);
        }

        private async Task<List<Article>> GetContentByCategory(string category, DateTime from)
        {
            List<Article> results = new List<Article>();

            HtmlWeb web = new HtmlWeb();

            HttpClient client = new HttpClient();

            int index = 0;
            int page = 1;

            while (true)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "https://www.9marks.org/wp-content/themes/9marks/get-posts.php");
                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("post_type", category.ToLower()));
                keyValues.Add(new KeyValuePair<string, string>("offset", "0"));
                keyValues.Add(new KeyValuePair<string, string>("paged", $"{page}"));
                request.Content = new FormUrlEncodedContent(keyValues);

                var response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var data = JsonConvert.DeserializeObject<GetPostsResponse>(await response.Content.ReadAsStringAsync());

                    foreach (var post in data.Posts)
                    {
                        var doc = new HtmlDocument();
                        doc.LoadHtml(post.Html);

                        var authorDate = doc.DocumentNode.SelectSingleNode("//strong[@class='meta']")?.InnerText?.Trim().Split(new char[] { '|' });
                        var author = authorDate[0]?.Trim();
                        if (author != null && author.StartsWith("By "))
                        {
                            author = author.Replace("By ", "").Trim();
                        }

                        DateTime date = new DateTime(1970, 1, 1);
                        if (authorDate.Count() > 1)
                        {
                            DateTime.TryParseExact(authorDate[authorDate.Length - 1].Trim(), new string[] { "MM.dd.yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
                        }

                        if (DateTime.Compare(date, from) < 0) return results;

                        var link = doc.DocumentNode.SelectSingleNode("//h3[@class='title']/a")?.Attributes["href"]?.Value;
                        var title = doc.DocumentNode.SelectSingleNode("//h3[@class='title']")?.InnerText?.Trim();
                        var summary = doc.DocumentNode.SelectSingleNode("//div[@class='content']")?.InnerText?.Trim();
                        var imageLinks = doc.DocumentNode.SelectSingleNode("//a[@class='thumb']/img")?.Attributes["srcset"]?.Value;

                        // Load article content
                        var html = web.Load(link);
                        var htmlContent = new HtmlDocument();
                        htmlContent.LoadHtml(html.Text);

                        var topics = htmlContent.DocumentNode.SelectNodes("//div[@class='meta-bottom']/div[@class='taxonomy']/span/a");

                        var paragraphs = htmlContent.DocumentNode.SelectNodes("//div[@id='main-content']//p");
                        StringBuilder sb = new StringBuilder();
                        if (paragraphs != null)
                        {
                            var items = paragraphs.Select(s => s.OuterHtml).ToList();
                            items.ForEach(item => sb.Append(item));
                        }
                        var text = sb.ToString();

                        var authors = new List<string>();
                        authors.AddRange(author.Split(","));
                        authors.ForEach(item => item = item.Trim());

                        var article = new Article
                        {
                            Author = authors,
                            Category = category,
                            Date = date,
                            ImageLink = imageLinks?.Split(",")?.LastOrDefault()?.Trim()?.Split(" ")?.FirstOrDefault()?.Trim(),
                            Link = link,
                            Summary = summary,
                            Title = title,
                            Topics = topics?.Select(s => s.InnerText).ToList(),
                            Text = text
                        };

                        results.Add(article);

                        Console.WriteLine($"{index++} Retrieving article {link}");
                    }

                    if (!data.HasMore) return results;
                }

                page++;
            }
        }

        private async Task CreateArticles(List<Article> articles)
        {
            var client = await new ClientHelper().GetLoggedClient();

            int index = 0;

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
                                Owner = "9Marks",
                                Title = article.Title,
                                Subtitle = article.Subtitle,
                                Summary = article.Summary,
                                Reference = article.Scripture,
                                Topics = article.Topics,
                                Characters = Program.TextLength(article.Title) +
                                   Program.TextLength(article.Subtitle) +
                                   Program.TextLength(article.Summary) +
                                   Program.TextLength(article.Text),
                                Words = Program.WordsCount(article.Title) +
                                    Program.WordsCount(article.Subtitle) +
                                    Program.WordsCount(article.Summary) +
                                    Program.WordsCount(article.Text)
                            },
                            Text = article.Text,
                        };

                        var postContent = new StringContent(JsonConvert.SerializeObject(bodyArticle), Encoding.UTF8, "application/json");

                        var responseArticle = await client.PostAsync("article", postContent);

                        if (responseArticle.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var createdData = await responseArticle.Content.ReadAsStringAsync();
                            var newArticle = JsonConvert.DeserializeObject<GlobalArticleDatabaseAPI.Models.CreateArticleResponse>(createdData);

                            Console.WriteLine($"{index} Added {article.Category} {article.Link}");
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

                index++;
            }
        }
    }

    public class GetPostsResponse
    {
        public string Status { get; set; }
        public int Page { get; set; }
        public int Num { get; set; }
        public int Found_posts { get; set; }
        public bool HasMore { get; set; }
        public List<Post> Posts { get; set; }
        public int Pages { get; set; }
        public Query Query { get; set; }
    }

    public class Post
    {
        public int Id { get; set; }
        public string Html { get; set; }
    }

    public class Query
    {
        public string Post_status { get; set; }
        public string Post_type { get; set; }
        public string Paged { get; set; }
        public List<string> Tax_query { get; set; }
        public List<string> Meta_query { get; set; }
    }
}
