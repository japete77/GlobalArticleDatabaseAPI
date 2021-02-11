using LambdaCore.Models;
using LambdaCore.Services.Wordpress.Implementations;
using LambdaCore.Services.Youtube.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LambdaCore.Services
{
    public class SolidJoysUpdater
    {
        private YouTubeService _youtubeService { get; }
        private WordpressService _wordpressService { get; }

        public SolidJoysUpdater()
        {
            _youtubeService = new YouTubeService();
            _wordpressService = new WordpressService();
        }

        public async Task UpdateSolidJoysEntries()
        {
            await _wordpressService.Authorize();

            Console.WriteLine("Retrieving youtube entries from playlist...");

            var solidJoysEntries = await GetSolidJoysEntries();

            Console.WriteLine("Retrieving WordPress Solid Joys Last Date...");

            var lastPostDate = await _wordpressService.GetLastDateSolidJoys();

            Console.WriteLine($"Last Solid Joys published on {lastPostDate}");

            foreach (var entry in solidJoysEntries)
            {
                var itemDate = GetSolidJoysDate(entry.snippet.title);

                if (itemDate > lastPostDate)
                {
                    var newPost = new WordpressEntry
                    {
                        Title = CleanSolidJoysTitle(entry.snippet.title),
                        Content = @$"{Config.Settings.YouTubeUrl}{entry.snippet.resourceId.videoId}

{CleanSolidJoysContent(entry.snippet.description)}
",
                        Author = Config.Settings.WordpressSolidJoysAuthor,
                        FeaturedMedia = Config.Settings.WordpressSolidJoysFeatureMediaId,
                        Format = "video",
                        Categories = new List<int> { Config.Settings.WordpressSolidJoysCategoryId },
                        Date = itemDate.Date,
                        Status = "publish",
                        Template = Config.Settings.WordpressSolidJoysTemplate,
                        Tags = new List<int> { Config.Settings.WordpressSolidJoysTag }
                    };

                    await _wordpressService.CreatePost(newPost);

                    Console.WriteLine($"Created Solid Joy Entry in WordPress: {newPost.Title} ...");
                }
            }
        }

        private async Task<List<Item>> GetSolidJoysEntries()
        {
            var entries = new List<Item>();

            string nextPageToken = null;

            do
            {
                var response = await _youtubeService.GetPlaylistItems(
                    Config.Settings.SolidJoysPlaylistId,
                    Config.Settings.SdjcChannelId,
                    Config.Settings.YoutubeApiKey,
                    nextPageToken);

                nextPageToken = response.nextPageToken;

                entries.AddRange(response.items);

            } while (nextPageToken != null);

            // Clean entries
            List<Item> cleanEntries = new List<Item>();
            entries.ForEach(data =>
            {
                if (!cleanEntries.Any(a => a.snippet.title == data.snippet.title) && data.snippet.title.ToLower() != "private video")
                {
                    cleanEntries.Add(data);
                }
            });

            return cleanEntries;
        }

        private string CleanSolidJoysContent(string content)
        {
            var result = Regex.Split(content, "\r\n|\r|\n");

            var clean = result.Skip(2).Where(s => !s.Contains("http://") && !s.Contains("https://")).ToList();

            var line = clean.Last();
            while (string.IsNullOrEmpty(line) || line.StartsWith("Encuentra más devocionales de John Piper"))
            {
                clean.RemoveAt(clean.Count - 1);
                line = clean.Last();
            }

            return string.Join('\n', clean);
        }

        private string CleanSolidJoysTitle(string title)
        {
            var sections = title.Split("-");
            if (sections.Length < 3) return "";
            else return sections[2].Trim();
        }

        private DateTime GetSolidJoysDate(string title)
        {
            var sections = title.Split("-");
            if (sections.Length < 3) return new DateTime();

            var monthDay = sections[1].Trim().Split(" ");
            var day = Convert.ToInt32(monthDay[1]);

            switch (monthDay[0].Trim())
            {
                case "Enero":
                    return new DateTime(2021, 1, day);
                case "Febrero":
                    return new DateTime(2021, 2, day);
                case "Marzo":
                    return new DateTime(2021, 3, day);
                case "Abril":
                    return new DateTime(2021, 4, day);
                case "Mayo":
                    return new DateTime(2021, 5, day);
                case "Junio":
                    return new DateTime(2021, 6, day);
                case "Julio":
                    return new DateTime(2020, 7, day);
                case "Agosto":
                    return new DateTime(2020, 8, day);
                case "Septiembre":
                    return new DateTime(2020, 9, day);
                case "Octubre":
                    return new DateTime(2020, 10, day);
                case "Noviembre":
                    return new DateTime(2020, 11, day);
                case "Diciembre":
                    return new DateTime(2020, 12, day);
                default:
                    return DateTime.Now;
            }
        }
    }
}
