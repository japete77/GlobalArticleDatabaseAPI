using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DesiringGodArticlesCrawler.Models;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;

namespace DesiringGodArticlesCrawler
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //ExtractSpanishArticlesFromDG("/Users/juanpablogonzalezjara/Desktop/articles/spanish_articles.json");
            // GetAuthors("/Users/juanpablogonzalezjara/Desktop/articles/authors.json");
            //var builder = new Automation.ArticlesBuilder();
            //builder.CreateArticles();

            //List<Article> articles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(@"C:\temp\pxe\articles\spanish_articles.json"));

            //var articlesPiper = articles.Where(w => w.Author == "John Piper" && w.Link.Contains("/articles/")).ToList();

            //var extractor = new ArticleExtractor();

            //var textEnglish = extractor.Extract(articlesPiper.First().Link.Replace("?lang=es", ""));
            //var textSpanish = extractor.Extract(articlesPiper.First().Link);

            // ************************************************************************
            // **************** Get All Articles from DG ******************************
            ////var topicsExtractor = new TopicsExtractor();
            ////var topics = topicsExtractor.Extract();

            //var authorsExtractor = new AuthorsExtractor();
            //var authors = authorsExtractor.Extract();

            //List<Article> dgResources = new List<Article>();
            //var resources = new ResourcesExtractor();

            ////foreach (var topic in topics)
            ////{
            ////    Console.WriteLine($"Extracting topic: {topic.Name}");
            ////    var data = resources.Extract($"{topic.Link}/all");
            ////    data.ForEach(item => item.Topic = topic.Name);
            ////    dgResources.AddRange(data);
            ////}

            //int authorIndex = 1;

            //Console.WriteLine($"Detected {authors.Count} authors");
            //foreach (var author in authors)
            //{
            //    Console.WriteLine($"{authorIndex} Extracting author: {author.Name}");
            //    var data = resources.Extract($"{author.Link}");
            //    data.ForEach(item => item.Author = author.Name);
            //    dgResources.AddRange(data);
            //    authorIndex++;
            //}

            //File.WriteAllText(@"C:/temp/pxe/dg_resources_by_author.json", JsonConvert.SerializeObject(dgResources));

            // *************************#################################**********************
            // *************************#################################**********************
            //List<Article> articlesByTopic = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(@"C:/temp/pxe/dg_resources.json"));
            //List<Article> articlesByAuthor = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(@"C:/temp/pxe/dg_resources_by_author.json"));
            // List<Article> articles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(@"C:/temp/pxe/articles/john-piper_articles_all.json"));

            var cHelper = new ClientHelper();
            var client = await cHelper.GetLoggedClient();

            int page = 1;
            int index = 1;
            while (true)
            {
                var result = await client.GetAsync($"articles?page={page}&pageSize=1000");

                var data = await result.Content.ReadAsStringAsync();
                var dbArticles = JsonConvert.DeserializeObject<GlobalArticleDatabaseAPI.Models.ArticleSearchResponse>(data);

                if (dbArticles.Articles.Count == 0) break;

                foreach (var a in dbArticles.Articles)
                {
                    a.Title = System.Web.HttpUtility.HtmlDecode(a.Title);
                    a.Subtitle = System.Web.HttpUtility.HtmlDecode(a.Subtitle);
                    a.Summary = System.Web.HttpUtility.HtmlDecode(a.Summary);

                    // Update Article
                    var httpContent = new StringContent(JsonConvert.SerializeObject(new GlobalArticleDatabaseAPI.Models.UpdateArticleRequest { Article = a }), Encoding.UTF8, "application/json");
                    await client.PutAsync("article", httpContent);

                    Console.WriteLine($"{index++} Update article");
                }

                page++;
            }


            //int index = 1;

            //var extractor = new ArticleExtractor();

            //// Create article
            //foreach (var a in articlesByAuthor)
            //{
            //    if (dbArticles.Articles.Where(w => w.SourceLink == a.Link).Count() == 0)
            //    {
            //        var dupes = articlesByTopic.Where(w => w.Link == a.Link).ToList();

            //        // Only create if exists
            //        var newResource = new GlobalArticleDatabaseAPI.Models.Article
            //        {
            //            Author = a.Author,
            //            Category = a.Category,
            //            Date = a.Date,
            //            HasText = true,
            //            ImageLink = a.ImageLink,
            //            Language = "us",
            //            Owner = "Desiring God",
            //            Reference = a.Scripture,
            //            SourceLink = a.Link,
            //            Subtitle = a.Subtitle,
            //            Summary = a.Summary,
            //            Title = a.Title,
            //            Topics = dupes.Select(s => s.Topic).ToList()
            //        };

            //        var f = $"C:/temp/pxe/resources/{newResource.SourceLink.Split('/').Last()}.txt";
            //        if (File.Exists(f))
            //        {
            //            var text = File.ReadAllText(f);

            //            // Create Article
            //            var httpContent = new StringContent(JsonConvert.SerializeObject(new GlobalArticleDatabaseAPI.Models.CreateArticleRequest { Article = newResource, Text = text }), Encoding.UTF8, "application/json");
            //            await client.PostAsync("article", httpContent);

            //            Console.WriteLine($"{index} Created {a.Link}");
            //        }
            //        //string text = extractor.Extract(newResource.SourceLink);
            //        //File.WriteAllText($"C:/temp/pxe/resources/{newResource.SourceLink.Split('/').Last()}.txt", text);

            //        //dbArticles.Articles.Add(newResource);
            //    }
            //    else
            //    {
            //        Console.WriteLine($"{index} Already exists {a.Link}");
            //    }

            //    index++;
            //}

            //// Update image link
            //foreach (var a in dbArticles.Articles)
            //{
            //    var source = articles.Where(w => w.Link == a.SourceLink).ToList();

            //    a.Author = a.Author.Trim();
            //    a.Category = a.Category?.Trim();
            //    a.Owner = a.Owner?.Trim();
            //    a.SourceLink = a.SourceLink.Trim();
            //    a.Subtitle = a.Subtitle?.Trim();
            //    a.Summary = a.Summary?.Trim();
            //    a.Title = a.Title?.Trim();
            //    if (source != null && source.Count > 0)
            //    {
            //        a.ImageLink = source.First().ImageLink?.Trim();
            //        a.Reference = source.First().Text?.Trim();
            //        a.Topics = source.Select(s => s.Topic).ToList();
            //    }
            //    else
            //    {
            //        a.Reference = null;
            //    }

            //    // Update Article
            //    var httpContent = new StringContent(JsonConvert.SerializeObject(new GlobalArticleDatabaseAPI.Models.UpdateArticleRequest { Article = a }), Encoding.UTF8, "application/json");
            //    await client.PutAsync("article", httpContent);

            //    Console.WriteLine($"{index} Updated {a.SourceLink}");

            //    index++;
            //}

        }

        static void GetAuthors(string filename)
        {
            HtmlWeb web = new HtmlWeb();

            List<string> authors = new List<string>();

            try
            {
                var html2 = web.Load($"https://www.desiringgod.org/authors");

                var doc = new HtmlDocument();
                doc.LoadHtml(html2.Text);

                var nodes = doc.DocumentNode.SelectNodes("//div[@class='author']/h4");

                if (nodes == null) return;

                Console.WriteLine($"Processing authors...");

                foreach (var node in nodes)
                {
                    var articleHtml = new HtmlDocument();
                    articleHtml.LoadHtml(node.InnerHtml);


                    var author = articleHtml.DocumentNode.SelectSingleNode(".")?.InnerText;
                    author = author.Trim();

                    authors.Add(author);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            File.WriteAllText(filename, JsonConvert.SerializeObject(authors));
        }

        /// <summary>
        /// Process translations
        /// </summary>
        /// <param name="sourceFile">list of articles links</param>
        /// <param name="author">authors: john-piper, tony-reinke, jon-bloom, david-mathis</param>
        static void ProcessTranslations(string sourceFile, string targetFolder, string author)
        {
            RegexOptions options = RegexOptions.None;
            Regex regex = new Regex("[ ]{2,}", options);

            List<Article> articles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(sourceFile));

            HtmlWeb web = new HtmlWeb();
            WebClient client = new WebClient();
            var restClient = new RestClient();

            string target = $"{targetFolder}/{author}/";

            for (int i = 0; i < articles.Count; i++)
            {
                Console.WriteLine($"{DateTime.Now.ToShortTimeString()} Processing article {i}: {articles[i].Link}");

                var date = articles[i].Date.ToString("yyyy-MM-dd");
                var fname = articles[i].Link.Split('/').Last();
                var basename = $"{i}_{date}_{fname}";

                // Save image if exists
                if (articles[i].ImageLink != null)
                {
                    var imgFile = $"{target}{basename}.jpg";
                    if (!File.Exists(imgFile))
                    {
                        client.DownloadFile(new Uri(articles[i].ImageLink), imgFile);
                    }
                }

                // Download english article
                HtmlDocument articleHtml;
                var htmlFile = $"{target}{basename}.html";
                if (File.Exists(htmlFile))
                {
                    articleHtml = new HtmlDocument();
                    articleHtml.Load(htmlFile);
                }
                else
                {
                    articleHtml = web.Load(articles[i].Link);
                    articleHtml.Save(htmlFile);
                }

                var doc = new HtmlDocument();
                doc.LoadHtml(articleHtml.Text);
                var body = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'resource__body')]");

                List<string> articleParagraphs = new List<string>();
                if (!string.IsNullOrEmpty(articles[i].Title))
                {
                    articleParagraphs.Add($"Title: {articles[i].Title}");
                }
                if (!string.IsNullOrEmpty(articles[i].Subtitle))
                {
                    articleParagraphs.Add($"Subtitle: {articles[i].Subtitle}");
                }
                if (!string.IsNullOrEmpty(articles[i].Summary))
                {
                    articleParagraphs.Add($"Summary: {articles[i].Summary}");
                }
                if (!string.IsNullOrEmpty(articles[i].Scripture))
                {
                    articleParagraphs.Add($"Scripture: {articles[i].Scripture}");
                }
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

                var translated = new List<Translations>();
                if (articleParagraphs.Count > 0)
                {
                    // Limit the number of lines
                    var blocks = articleParagraphs.Select((str, index) => new { str, index })
                        .GroupBy(x => x.index / 100)
                        .Select(g => g.Select(x => x.str).ToList())
                        .ToList();

                    foreach (var block in blocks)
                    {
                        // Call Google API to translate and save to disk
                        translated.AddRange(Translate(restClient, block));
                    }
                }

                // Save to disk
                StringBuilder englishText = new StringBuilder();
                articleParagraphs.ForEach(item =>
                {
                    var line = item.Trim();
                    if (!string.IsNullOrEmpty(line))
                    {
                        englishText.AppendLine(line);
                        englishText.AppendLine();
                    }
                });

                File.WriteAllText($"{target}{basename}.en.txt", englishText.ToString());

                StringBuilder spanishText = new StringBuilder();
                translated.ForEach(item =>
                {
                    spanishText.AppendLine(System.Web.HttpUtility.HtmlDecode(item.translatedText));
                    spanishText.AppendLine();
                });

                File.WriteAllText($"{target}{basename}.es.txt", spanishText.ToString());
            }
        }

        static List<Translations> Translate(RestClient restClient, List<string> text)
        {
            bool retry = true;
            IRestResponse<TranslateData> response = null;

            while (retry)
            {
                retry = false;
                var request = new RestRequest();
                request.Method = Method.POST;
                request.Resource = "https://translation.googleapis.com/language/translate/v2?key=AIzaSyAq4YjOz-ulbEsEkYi6eqKBY0Usks8fLtk";
                request.RequestFormat = DataFormat.Json;
                var data = new TranslateRequest
                {
                    q = text,
                    target = "es"
                };

                request.AddJsonBody(data);

                response = restClient.Post<TranslateData>(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"Error: {JsonConvert.SerializeObject(response.Data)}");
                    Console.WriteLine($"Retry in 10 seconds...");
                    retry = true;
                    Thread.Sleep(10000);
                }
            }

            return response?.Data?.data?.translations;
        }

        static List<Translations> TranslateV3(RestClient restClient, List<string> text)
        {
            TranslationServiceClient client = TranslationServiceClient.Create();
            TranslateTextRequest request = new TranslateTextRequest();
            request.TargetLanguageCode = "es-ES";
            request.Parent = new ProjectName("desiringgod").ToString();
            request.Contents.AddRange(text);

            TranslateTextResponse response = client.TranslateText(request);

            return response.Translations.Select(s => new Translations
            {
                translatedText = s.TranslatedText,
                detectedSourceLanguage = s.DetectedLanguageCode
            }).ToList();
        }

        static void ExtractPending(string targetFile)
        {
            List<Article> enArticles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText("dg_articles_en.json"));
            List<Article> esArticles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText("dg_articles_es.json"));

            List<DateTime> dates = esArticles.Select(s => s.Date).ToList();

            var pendingToTranslate = enArticles.Where(w => !dates.Contains(w.Date)).ToList();

            File.WriteAllText(targetFile, JsonConvert.SerializeObject(pendingToTranslate));
        }

        static void ExtractSpanishArticlesFromDG(string filename)
        {
            HtmlWeb web = new HtmlWeb();

            int page = 1;

            List<Article> articles = new List<Article>();
            if (File.Exists(filename))
            {
                articles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(filename));
            }

            while (true)
            {
                try
                {
                    var html2 = web.Load($"https://www.desiringgod.org/languages/spanish?page={page}");

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html2.Text);

                    var nodes = doc.DocumentNode.SelectNodes("//div[@class='card-list-view']/div[starts-with(@class,'card--resource')]");

                    if (nodes == null) break;

                    Console.WriteLine($"Processing page {page}...");

                    foreach (var node in nodes)
                    {
                        var articleHtml = new HtmlDocument();
                        articleHtml.LoadHtml(node.InnerHtml);

                        var type = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__labels-label']")?.InnerText;
                        var author = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card__author']")?.InnerText;
                        author = author.Trim();

                        var link = articleHtml.DocumentNode.SelectSingleNode("//a[@class='card__shadow']")?.Attributes["href"]?.Value;

                        articles.Add(
                            new Article
                            {
                                Author = author,
                                Link = $"https://www.desiringgod.org{link}",
                                Title = articleHtml.DocumentNode.SelectSingleNode("//h2[@class='card--resource__title']")?.InnerText,
                                Subtitle = articleHtml.DocumentNode.SelectSingleNode("//h3[@class='card--resource__subtitle']")?.InnerText,
                                Date = DateTime.Parse(articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__date']")?.InnerText),
                                Scripture = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']/div[@class='card--resource__scripture']")?.InnerText.Trim()
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

            File.WriteAllText(filename, JsonConvert.SerializeObject(articles));
        }

        static void ExtractEnglishArticles(string author, string filename)
        {
            HtmlWeb web = new HtmlWeb();

            int page = 1;

            List<Article> articles = new List<Article>();
            if (File.Exists(filename))
            {
                articles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(filename));
            }

            while (true)
            {
                try
                {
                    var html = web.Load($"https://www.desiringgod.org/authors/{author}/articles?page={page}");

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html.Text);

                    var nodes = doc.DocumentNode.SelectNodes("//div[@class='card-list-view']/div[starts-with(@class,'card--resource')]");

                    if (nodes == null) break;

                    Console.WriteLine($"Processing page {page}...");

                    foreach (var node in nodes)
                    {
                        var articleHtml = new HtmlDocument();
                        articleHtml.LoadHtml(node.InnerHtml);

                        var link = articleHtml.DocumentNode.SelectSingleNode("//a[@class='card__shadow']")?.Attributes["href"]?.Value;

                        articles.Add(
                            new Article
                            {
                                ImageLink = articleHtml.DocumentNode.SelectSingleNode("//a[@class='card__shadow']/div[@class='card__inner']/div/img")?.Attributes["data-src"]?.Value,
                                Link = $"https://www.desiringgod.org{link}",
                                Title = articleHtml.DocumentNode.SelectSingleNode("//h2[@class='card--resource__title']")?.InnerText,
                                Subtitle = articleHtml.DocumentNode.SelectSingleNode("//h3[@class='card--resource__subtitle']")?.InnerText,
                                Date = DateTime.Parse(articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__date']")?.InnerText),
                                Scripture = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']/div[@class='card--resource__scripture']")?.InnerText.Trim()
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

            File.WriteAllText(filename, JsonConvert.SerializeObject(articles));
        }
    }










}
