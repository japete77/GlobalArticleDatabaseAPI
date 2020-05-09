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
using System.Web;
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
            // var updater = new DGContentUpdater();
            // await updater.Update(new DateTime(2020, 5, 1));
            var soldadosExtractor = new SoldadosContentExtractor();
            await soldadosExtractor.Extract();
        }

        static void SetupParagraphsFromFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.txt");

            int index = 0;
            foreach (var f in files)
            {
                StringBuilder sb = new StringBuilder();
                var text = File.ReadAllText(f);

                if (text != null)
                {
                    var m = Regex.Match(text, @"<[^>]*?>");
                    if (!m.Success)
                    {
                        var paragraphs = Regex.Split(text, @"(\r\n?|\n){2}")
                              .Where(p => p.Any(char.IsLetterOrDigit));

                        foreach (var paragraph in paragraphs)
                        {
                            sb.AppendLine($"<p>{paragraph}</p>");
                        }

                        // Update Article text
                        File.WriteAllText(f, sb.ToString());

                        Console.WriteLine($"{index}, Update article text {f}");
                    }
                    else
                    {
                        Console.WriteLine($"{index}, >>> Article already updated {f}");
                    }
                }
                index++;
            }
        }

        static async Task FixUrls()
        {
            var cHelper = new ClientHelper();
            HttpClient client;

            client = await cHelper.GetLoggedClient();

            int page = 1;
            int index = 1;
            while (true)
            {
                var result = await client.GetAsync($"articles?page={page}&pageSize=200");

                var data = await result.Content.ReadAsStringAsync();

                var dbArticles = JsonConvert.DeserializeObject<GlobalArticleDatabaseAPI.Models.ArticleSearchResponse>(data);

                if (dbArticles.Articles.Count == 0) break;

                foreach (var a in dbArticles.Articles)
                {
                    if (!string.IsNullOrEmpty(a.ImageLink))
                    {
                        if (a.ImageLink.Contains("&amp;"))
                        {
                            a.ImageLink = HttpUtility.HtmlDecode(a.ImageLink);

                            var httpContent = new StringContent(JsonConvert.SerializeObject(new GlobalArticleDatabaseAPI.Models.UpdateArticleRequest { Article = a }), Encoding.UTF8, "application/json");
                            await client.PutAsync("article", httpContent);
                            Console.WriteLine($"{index}, Updated article image link {a.ImageLink}");
                        }
                        else
                        {
                            Console.WriteLine($"{index}, skipped - No fix needed...");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{index}, skipped - No image link...");
                    }

                    index++;
                }

                page++;
            }
        }

        static async Task SetupParagraphs()
        {
            var cHelper = new ClientHelper();
            HttpClient client;

            int page = 1;
            int index = 1;
            while (true)
            {
                client = await cHelper.GetLoggedClient();

                var result = await client.GetAsync($"articles?page={page}&pageSize=500");

                var data = await result.Content.ReadAsStringAsync();

                var dbArticles = JsonConvert.DeserializeObject<GlobalArticleDatabaseAPI.Models.ArticleSearchResponse>(data);

                if (dbArticles.Articles.Count == 0) break;

                foreach (var a in dbArticles.Articles)
                {
                    var httpTextResponse = await client.GetAsync($"article/{a.Id}/text");
                    if (httpTextResponse.StatusCode == HttpStatusCode.OK)
                    {
                        StringBuilder sb = new StringBuilder();
                        var text = JsonConvert.DeserializeObject<string>(await httpTextResponse.Content.ReadAsStringAsync());

                        if (text != null)
                        {
                            var m = Regex.Match(text, @"<[^>]*?>");
                            if (!m.Success)
                            {
                                var paragraphs = Regex.Split(text, @"(\r\n?|\n){2}")
                                      .Where(p => p.Any(char.IsLetterOrDigit));

                                foreach (var paragraph in paragraphs)
                                {
                                    sb.AppendLine($"<p>{paragraph}</p>");
                                }

                                // Update Article text
                                var httpContent = new StringContent(JsonConvert.SerializeObject(new GlobalArticleDatabaseAPI.Models.UpdateArticleTextRequest { Id = a.Id, Text = sb.ToString() }), Encoding.UTF8, "application/json");
                                await client.PutAsync("article/text", httpContent);

                                Console.WriteLine($"{index}, Update article text {a.SourceLink}");
                            }
                        }

                        if (a.Translations != null)
                        {
                            foreach (var t in a.Translations)
                            {
                                var httpTextResponseTranslation = await client.GetAsync($"article/{a.Id}/text?language={t.Language}");
                                if (httpTextResponseTranslation.StatusCode == HttpStatusCode.OK)
                                {
                                    var translationText = JsonConvert.DeserializeObject<string>(await httpTextResponseTranslation.Content.ReadAsStringAsync());

                                    if (translationText != null)
                                    {
                                        var m = Regex.Match(translationText, @"<[^>]*?>");
                                        if (!m.Success)
                                        {
                                            StringBuilder translationString = new StringBuilder();
                                            var translationParagraphs = Regex.Split(translationText, @"(\r\n?|\n){2}")
                                                  .Where(p => p.Any(char.IsLetterOrDigit));

                                            foreach (var paragraph in translationParagraphs)
                                            {
                                                translationString.AppendLine($"<p>{paragraph}</p>");
                                            }

                                            // Update Article text
                                            var httpContentUpdateTraslation = new StringContent(JsonConvert.SerializeObject(new GlobalArticleDatabaseAPI.Models.UpdateTranslationTextRequest { ArticleId = a.Id, Language = t.Language, Text = translationString.ToString() }), Encoding.UTF8, "application/json");
                                            await client.PutAsync("translation/text", httpContentUpdateTraslation);

                                            Console.WriteLine($"{index}, Update article translation '{t.Language}'text {a.SourceLink}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{index}, Article text NOT FOUND: {a.TextLink}");
                    }

                    index++;
                }

                page++;
            }
        }

        static async Task SetupWordCount()
        {
            var cHelper = new ClientHelper();
            HttpClient client;

            int page = 1;
            int index = 1;
            while (true)
            {
                client = await cHelper.GetLoggedClient();

                var result = await client.GetAsync($"articles?page={page}&pageSize=500");

                var data = await result.Content.ReadAsStringAsync();

                var dbArticles = JsonConvert.DeserializeObject<GlobalArticleDatabaseAPI.Models.ArticleSearchResponse>(data);

                if (dbArticles.Articles.Count == 0) break;

                foreach (var a in dbArticles.Articles)
                {
                    if (a.Words == 0)
                    {
                        var httpTextResponse = await client.GetAsync($"article/{a.Id}/text");
                        if (httpTextResponse.StatusCode == HttpStatusCode.OK)
                        {
                            var text = JsonConvert.DeserializeObject<string>(await httpTextResponse.Content.ReadAsStringAsync());

                            // update size
                            a.Characters = TextLength(a.Title) +
                                           TextLength(a.Subtitle) +
                                           TextLength(a.Summary) +
                                           TextLength(text);

                            a.Words = WordsCount(a.Title) +
                                      WordsCount(a.Subtitle) +
                                      WordsCount(a.Summary) +
                                      WordsCount(text);

                            // Update Article
                            var httpContent = new StringContent(JsonConvert.SerializeObject(new GlobalArticleDatabaseAPI.Models.UpdateArticleRequest { Article = a }), Encoding.UTF8, "application/json");
                            await client.PutAsync("article", httpContent);

                            Console.WriteLine($"{index}, C={a.Characters}, W={a.Words} Update article {a.SourceLink}");
                        }
                        else
                        {
                            Console.WriteLine($"{index}, Article text NOT FOUND: {a.TextLink}");
                        }

                        index++;
                    }
                }

                page++;
            }
        }

        public static int WordsCount(string text)
        {
            if (text != null)
            {
                MatchCollection collection = Regex.Matches(text, @"[\S]+");
                return collection.Count;
            }

            return 0;
        }

        public static int TextLength(string text)
        {
            return text != null ? text.Length : 0;
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
