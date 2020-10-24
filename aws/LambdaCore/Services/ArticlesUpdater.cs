using GlobalArticleDatabaseAPI.Models;
using LambdaCore.Models;
using LambdaCore.Services.Gadb.Implementations;
using LambdaCore.Services.Wordpress.Implementations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LambdaCore.Services
{
    public class ArticlesUpdater
    {
        private WordpressService _wordpressService { get; }

        private List<WordpressAuthor> _wordpressAuthors;

        private List<WordpressTopic> _wordpressTopics;

        private Dictionary<string, string> _topicsTranslator;

        private const string PUBLISHER_PXE = "PxE";

        public ArticlesUpdater()
        {
            _wordpressService = new WordpressService();
        }

        public async Task UpdateArticleEntries()
        {
            await _wordpressService.Authorize();

            // Get scheduled articles
            List<ArticleSchedule> articleScheduled = await GetContentFromUrl<List<ArticleSchedule>>(Config.Settings.ArticlesScheduleUrl);

            // Get last article
            var lastArticle = await _wordpressService.GetLastArticle();
            var lastArticleDate = lastArticle != null ? lastArticle.Date : new DateTime();

            Console.WriteLine($"Last article date published {lastArticleDate}");

            // Filter out future articles
            var article2Publish = articleScheduled
                .Where(w => w.Date <= DateTime.Now && w.Date > lastArticleDate && !string.IsNullOrEmpty(w.Article))
                .OrderBy(o => o.Date)
                .ToList();

            if (article2Publish.Count > 0)
            {
                var gadbService = new GadbService();

                await gadbService.Login(Config.Settings.GadbUsername, Config.Settings.GadbPassword);

                // Loop to update until the article exists in wordpress...
                foreach (var item in article2Publish)
                {
                    if (!string.IsNullOrEmpty(item.Article))
                    {
                        var split = item.Article.Split("/");

                        var article = await gadbService.GetArticle(split[split.Length - 1]);

                        var translation = article.Translations.Where(w => w.Language.ToLower().Equals("es")).FirstOrDefault();

                        var topics = await TranslateTopics(article.Topics);

                        var newPost = new WordpressEntry
                        {
                            Author = await ResolveAuthor(article.Author.FirstOrDefault()),
                            Title = translation.Title,
                            Content = await GetContentFromUrl(translation.TextLink),
                            Format = "standard",
                            Template = Config.Settings.WordpressArticleTemplate,
                            Categories = new List<int> { Config.Settings.WordpressArticleCategoryId },
                            Tags = await ResolveTopics(topics),
                            Date = item.Date,
                            Status = "publish",
                            FeaturedMedia = await GetFeaturedMedia(article.ImageLink)
                        };

                        var createdPost = await _wordpressService.CreatePost(newPost);

                        Console.WriteLine($"Created new article: {translation.Title}");

                        bool registeredPublication =
                            translation.Publications != null &&
                            translation.Publications
                                .Where(w => w.Publisher == PUBLISHER_PXE &&
                                            DateTime.Compare(w.Date, item.Date) == 0)
                                .Count() > 0;

                        if (registeredPublication)
                        {
                            // Delete previous publication
                            await gadbService.DeletePublishDate(article.Id, translation.Language, PUBLISHER_PXE);
                        }

                        // Add publish entry in gadb database
                        await gadbService.AddPublishDate(new CreatePublicationRequest
                        {
                            ArticleId = article.Id,
                            Language = translation.Language,
                            Publication = new Publication
                            {
                                Date = item.Date,
                                Link = createdPost.Link,
                                Publisher = PUBLISHER_PXE,
                                Status = "Published"
                            }
                        });
                    }
                }
            }
            else
            {
                Console.WriteLine("No new articles to be published found");
            }
        }

        private async Task<List<string>> TranslateTopics(List<string> topics)
        {
            List<string> result = new List<string>();

            if (_topicsTranslator == null)
            {
                _topicsTranslator = await LoadTopicsDictionary();
            }

            foreach (var topic in topics)
            {
                if (_topicsTranslator.ContainsKey(topic))
                {
                    result.Add(_topicsTranslator[topic]);
                }
                else
                {
                    result.Add(topic);
                }
            }

            return result;
        }

        private async Task<Dictionary<string, string>> LoadTopicsDictionary()
        {
            var contentEN = await GetContentFromUrl(Config.Settings.ArticlesTopicsENUrl);
            var contentES = await GetContentFromUrl(Config.Settings.ArticlesTopicsESUrl);

            var topicsEN = Regex.Split(contentEN, "\r\n|\r|\n");
            var topicsES = Regex.Split(contentES, "\r\n|\r|\n");

            if (topicsEN.Length != topicsES.Length) throw new Exception("Invalid topics dictionary");

            var result = new Dictionary<string, string>();
            for (int i = 0; i < topicsEN.Length; i++)
            {
                result.Add(topicsEN[i], topicsES[i]);
            }

            return result;
        }

        /// <summary>
        /// Upload image to wordpress and return feature media id
        /// </summary>
        /// <param name="url">Url of the image</param>
        /// <returns></returns>
        private async Task<int> GetFeaturedMedia(string url)
        {
            if (string.IsNullOrEmpty(url)) return default;

            //// Download image
            var data = await GetContentFromUrlBinary(url);

            var queryParamsIndex = url.IndexOf("?");
            if (queryParamsIndex > 0)
            {
                url = url.Substring(0, url.IndexOf("?"));
            }

            string filename = url.Substring(url.LastIndexOf("/") + 1);

            // Upload to wordpress
            var mediaEntry = await _wordpressService.CreateMediaFile(filename, data);

            // Return feature media id
            return mediaEntry.Id;
        }

        /// <summary>
        /// Resolve topic list into spanish and return id´s of the items from wordpress
        /// </summary>
        /// <param name="topics">List of topics in english</param>
        /// <returns>List of id´s from wordpress</returns>
        private async Task<List<int>> ResolveTopics(List<string> topics)
        {
            List<int> result = new List<int>();

            if (_wordpressTopics == null)
            {
                _wordpressTopics = await _wordpressService.GetTopics();
            }

            foreach (var topic in topics)
            {
                var match = _wordpressTopics.Where(w => w.Name.ToLower() == topic.ToLower()).FirstOrDefault();

                if (match == null)
                {
                    // Create topic and add to the list
                    var newTopic = await _wordpressService.CreateTag(new WordpressTopic { Name = topic });

                    _wordpressTopics.Add(newTopic);

                    result.Add(newTopic.Id);
                }
                else
                {
                    result.Add(match.Id);
                }
            }

            return result;
        }

        /// <summary>
        /// Search for author in wordpress and return id
        /// </summary>
        /// <param name="author">Author</param>
        /// <returns>Author Id from Wordpress</returns>
        private async Task<int> ResolveAuthor(string author)
        {
            if (string.IsNullOrEmpty(author))
            {
                throw new Exception($"Can´t creare article: Invalid author");
            }

            if (_wordpressAuthors == null)
            {
                _wordpressAuthors = await _wordpressService.GetAuthors();
            }

            var match = _wordpressAuthors.Where(w => w.Name.ToLower() == author.ToLower()).FirstOrDefault();

            if (match == null)
            {
                throw new Exception($"Author {author} not found in wordpress");
            }

            return match.Id;
        }

        private async Task<T> GetContentFromUrl<T>(string url)
        {
            T result;

            var webRequest = WebRequest.Create(url);
            using (var response = await webRequest.GetResponseAsync())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                result = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
            }

            return result;
        }

        private async Task<string> GetContentFromUrl(string url)
        {
            string result;

            var webRequest = WebRequest.Create(url);
            using (var response = await webRequest.GetResponseAsync())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        private async Task<byte[]> GetContentFromUrlBinary(string url)
        {
            byte[] result;
            byte[] buffer = new byte[4096];

            var webRequest = WebRequest.Create(url);
            using (var response = await webRequest.GetResponseAsync())
            using (var content = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = content.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, count);

                } while (count != 0);

                result = memoryStream.ToArray();
            }

            return result;
        }
    }
}
