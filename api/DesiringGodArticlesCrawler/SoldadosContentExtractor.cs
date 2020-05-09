using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DesiringGodArticlesCrawler
{
    public class SoldadosContentExtractor
    {
        private string sdj_file = "sdj_data.json";
        private int limit = 100;

        public async Task Extract()
        {
            var client = new HttpClient();

            string json;
            string beforeFilter = "";
            List<SDJArticle> articles = new List<SDJArticle>();

            if (File.Exists(sdj_file))
            {
                json = File.ReadAllText(sdj_file);
            }
            else
            {
                int page = 1;
                while (true)
                {
                    List<SDJArticle> currentPage;
                    var responseBoards = await client.GetAsync($"https://api.trello.com/1/boards/575ea9bc39769cb6a89a0253/cards/closed?stickers=true&attachments=true&pluginData=true&customFieldItems=true&token=e87cef451e99db9df236ef9cc12095892b49a46fdeeba1d4e18c11a88335d41b&key=38653b44bf4c71ba085c3c6d5541527c&limit={limit}{beforeFilter}");
                    if (responseBoards.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        json = await responseBoards.Content.ReadAsStringAsync();
                        currentPage = JsonConvert.DeserializeObject<List<SDJArticle>>(json);
                        if (currentPage.Count == 0) break;
                    }
                    else
                    {
                        throw new Exception("Error retrieving data from SDJ");
                    }

                    articles.AddRange(currentPage);
                    Console.WriteLine($"Retrieved page {page++} # total articles retrieved: {articles.Count}");

                    var unixTimestamp = (currentPage[currentPage.Count - 1].DateLastActivity.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;

                    beforeFilter = $"&before={unixTimestamp}";
                }

                File.WriteAllText(sdj_file, JsonConvert.SerializeObject(articles));
            }
        }
    }

    public class SDJArticle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime DateLastActivity { get; set; }
        public List<Label> Labels { get; set; }
        public List<Attachment> Attachments { get; set; }
        public Cover Cover { get; set; }
    }

    public class Label
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Attachment
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsUpload { get; set; }
        public string Url { get; set; }
    }

    public class Cover
    {
        public List<Scaled> Scaled { get; set; }
    }

    public class Scaled
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }
}
