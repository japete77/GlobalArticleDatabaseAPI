using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp;

namespace DesiringGodArticlesCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //ExtractSpanishArticlesFromDG("/Users/juanpablogonzalezjara/Desktop/articles/spanish_articles.json");
            // GetAuthors("/Users/juanpablogonzalezjara/Desktop/articles/authors.json");
            //var builder = new Automation.ArticlesBuilder();
            //builder.CreateArticles();

            List<Article> articles = JsonConvert.DeserializeObject<List<Article>>(File.ReadAllText(@"C:\temp\pxe\articles\spanish_articles.json"));

            var articlesPiper = articles.Where(w => w.Author == "John Piper" && w.Link.Contains("/articles/")).ToList();

            var extractor = new ArticleExtractor();

            var textEnglish = extractor.Extract(articlesPiper.First().Link.Replace("?lang=es", ""));
            var textSpanish = extractor.Extract(articlesPiper.First().Link);
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
                if (!string.IsNullOrEmpty(articles[i].Text))
                {
                    articleParagraphs.Add($"Summary: {articles[i].Text}");
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
                                Text = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']")?.InnerText
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
                                Text = articleHtml.DocumentNode.SelectSingleNode("//div[@class='card--resource__text']")?.InnerText
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

    public class Article
    {
        public string Author { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Link { get; set; }
        public string ImageLink { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }
    }

    public class TranslateRequestV3
    {
        public string sourceLanguageCode { get; set; }
        public string targetLanguageCode { get; set; }
        public List<string> contents { get; set; }
    }

    public class TranslateRequest
    {
        public List<string> q { get; set; }
        public string target { get; set; }
    }

    public class TranslateData
    {
        public TranslateResponse data { get; set; }
    }

    public class TranslateResponse
    {
        public List<Translations> translations { get; set; }
    }

    public class Translations
    {
        public string translatedText { get; set; }
        public string detectedSourceLanguage { get; set; }
    }
}
