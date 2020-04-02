using GlobalArticleDatabaseAPI.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automation
{
    internal class ArticleExtract
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Summary { get; set; }
        public string Text { get; set; }
    }

    public class ArticlesBuilder
    {
        private string LoadImageBase64(int index, DesiringGodArticlesCrawler.Article article)
        {
            string filename = $"C:/temp/pxe/articles/john-piper/{index}_{article.Date.ToString("yyyy-MM-dd")}_{article.Link.Split('/').Last()}.jpg";
            if (File.Exists(filename))
            {
                var data = File.ReadAllBytes(filename);
                return Convert.ToBase64String(data);
            }

            return null;
        }

        private ArticleExtract LoadText(int index, string language, DesiringGodArticlesCrawler.Article article)
        {
            string filename = $"C:/temp/pxe/articles/john-piper/{index}_{article.Date.ToString("yyyy-MM-dd")}_{article.Link.Split('/').Last()}.{language}.txt";
            if (File.Exists(filename))
            {
                var result = new ArticleExtract();

                StringBuilder sb = new StringBuilder();
                var data = File.ReadAllLines(filename);
                for (int i = 0; i < data.Length; i++)
                {
                    // Remove title, subtitle and summary
                    if (i >= 0 && i < 6)
                    {
                        if (string.IsNullOrEmpty(data[i])) continue;
                        else if (data[i].StartsWith("Title:")) result.Title = data[i].Length > 7 ? data[i].Substring(7) : "";
                        else if (data[i].StartsWith("Subtitle:")) result.Subtitle = data[i].Length > 10 ? data[i].Substring(10) : "";
                        else if (data[i].StartsWith("Summary:")) result.Summary = data[i].Length > 9 ? data[i].Substring(9) : "";
                        else if (data[i].StartsWith("Título:")) result.Title = data[i].Length > 8 ? data[i].Substring(8) : "";
                        else if (data[i].StartsWith("Subtítulo:")) result.Subtitle = data[i].Length > 11 ? data[i].Substring(11) : "";
                        else if (data[i].StartsWith("Resumen:")) result.Summary = data[i].Length > 9 ? data[i].Substring(9) : "";
                        else sb.AppendLine(data[i]);
                    }
                    else
                    {
                        sb.AppendLine(data[i]);
                    }
                }

                result.Text = sb.ToString();

                return result;
            }

            return null;
        }


        public void CreateArticles()
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://ig24hiba4k.execute-api.eu-west-1.amazonaws.com/Prod/api/v1/");
            // client.BaseUrl = new Uri("http://localhost:5000/api/v1/");

            List<DesiringGodArticlesCrawler.Article> articles = JsonConvert.DeserializeObject<List<DesiringGodArticlesCrawler.Article>>(File.ReadAllText("C:/temp/pxe/articles/john-piper_articles.json"));

            for (int index = 0; index < articles.Count; index++)
            {
                if (articles[index] == null) continue;

                Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Creating article {index} ... {articles[index].Link}");

                // Load text english
                //Console.WriteLine("Load text ...");
                var textEnglish = LoadText(index, "en", articles[index]);
                if (textEnglish == null)
                {
                    Console.WriteLine($">>> Not found content for {index} at {articles[index].Link}");
                    continue;
                }

                var image = LoadImageBase64(index, articles[index]);
                if (image == null) Console.WriteLine("Image is null...");

                // Create article
                //Console.WriteLine("Call API...");
                RestRequest requestArticle = new RestRequest();
                requestArticle.Resource = "article";
                var bodyArticle = new CreateArticleRequest
                {
                    Article = new Article
                    {
                        Author = "John Piper",
                        Category = "Article",
                        Date = articles[index].Date,
                        HasText = textEnglish != null,
                        HasImage = image != null,
                        SourceLink = articles[index].Link,
                        Language = "en",
                        Owner = "Desiring God",
                        Title = articles[index].Title,
                        Subtitle = articles[index].Subtitle,
                        Summary = articles[index].Text,
                    },
                    Text = textEnglish?.Text,
                    ImageBase64 = image,
                };
                requestArticle.AddJsonBody(bodyArticle);

                var responseArticle = client.Post<CreateArticleResponse>(requestArticle);

                //Console.WriteLine(JsonConvert.SerializeObject(bodyArticle));

                //Console.WriteLine("Response from API ...");
                if (responseArticle.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($">>> Error {responseArticle.StatusCode} creating article for article {articles[index].Link}");
                    continue;
                }

                // Create translation
                //Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss")} Creating translation...");

                // Load text spanish
                //Console.WriteLine("Load translation text...");
                var textSpanish = LoadText(index, "es", articles[index]);

                //Console.WriteLine("Call translation API...");
                RestRequest requestTranslation = new RestRequest();
                requestTranslation.Resource = "translation";
                var bodyTranslation = new CreateTranslationRequest
                {
                    ArticleId = responseArticle.Data.Article.Id,
                    Translation = new Translation
                    {
                        Date = new DateTime(2020, 3, 21),
                        HasText = true,
                        Language = "es",
                        Status = "Pending review",
                        Title = textSpanish.Title,
                        Subtitle = textSpanish.Subtitle,
                        Summary = textSpanish.Summary,
                        TranslatedBy = "Google Cloud Translation"
                    },
                    Text = textSpanish.Text,
                };
                requestTranslation.AddJsonBody(bodyTranslation);

                var responseTranslation = client.Post(requestTranslation);

                //bodyTranslation.Text = null;
                //Console.WriteLine(JsonConvert.SerializeObject(bodyTranslation));

                //Console.WriteLine("Response from translation API...");
                if (responseTranslation.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($">>> Error {responseTranslation.StatusCode} creating translation for article {articles[index].Link}");
                }

                //Console.WriteLine("Press any key to process next item...");
                //Console.ReadKey();
            }

        }
    }
}
