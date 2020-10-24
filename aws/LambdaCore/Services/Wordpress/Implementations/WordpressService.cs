using LambdaCore.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LambdaCore.Services.Wordpress.Implementations
{
    public class WordpressService
    {
        int PAGE_SIZE = 25;

        private readonly HttpClient _client;

        private string JwtToken;

        public WordpressService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri($"{Config.Settings.WordpressApiUrl}");
        }

        public async Task Authorize()
        {
            var client = new RestClient($"{Config.Settings.WordpressJwtUrl}");
            client.Timeout = -1;

            var request = new RestRequest(Method.POST);
            request.AddParameter("username", $"{Config.Settings.WordpressUsername}");
            request.AddParameter("password", $"{Config.Settings.WordpressPassword}");

            IRestResponse response = await client.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Error creating media: {response.Content}");
            }

            var jwt = JsonConvert.DeserializeObject<JwtResponse>(response.Content);

            JwtToken = jwt.Token;

            // Setup jwt token
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {JwtToken}");
            _client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
        }

        public async Task<WordpressEntryResult> CreatePost(WordpressEntry entry)
        {
            Dictionary<string, string> form = new Dictionary<string, string>
            {
                { "title",  entry.Title },
                { "content",  entry.Content },
                { "author",  entry.Author.ToString() },
                { "format",  entry.Format },
                { "categories",  string.Join(",", entry.Categories) },
                { "date",  entry.Date.ToString("yyyy-MM-ddTHH:mm:ss") },
                { "status",  entry.Status },
                { "template",  entry.Template }
            };

            if (entry.FeaturedMedia > 0)
            {
                form.Add("featured_media", entry.FeaturedMedia.ToString());
            }

            if (entry.Tags.Count > 0)
            {
                form.Add("tags", string.Join(",", entry.Tags));
            }

            var result = await _client.PostAsync("posts", new FormUrlEncodedContent(form));
            var content = await result.Content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode) throw new Exception($"Error creating article post {entry.Title}: {content}");

            var postEntry = JsonConvert.DeserializeObject<WordpressEntryResult>(content);

            return postEntry;
        }

        public async Task<WordpressMedia> CreateMediaFile(string filename, byte[] data)
        {
            var client = new RestClient($"{Config.Settings.WordpressApiUrl}media");
            client.Timeout = -1;

            var extension = Path.GetExtension(filename).Substring(1);

            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Disposition", $"attachment; filename=\"{filename}\"");
            request.AddHeader("Authorization", $"Bearer {JwtToken}");
            request.AddHeader("Content-Type", $"image/{extension}");
            request.AddParameter($"image/{extension}", data, ParameterType.RequestBody);

            IRestResponse response = await client.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception($"Error creating media: {response.Content}");
            }

            var mediaEntry = JsonConvert.DeserializeObject<WordpressMedia>(response.Content);

            return mediaEntry;
        }

        public async Task<WordpressTopic> CreateTag(WordpressTopic topic)
        {
            var form = new Dictionary<string, string>()
            {
                { "name",  topic.Name }
            };

            var result = await _client.PostAsync("tags", new FormUrlEncodedContent(form));
            var content = await result.Content.ReadAsStringAsync();

            var newTopic = JsonConvert.DeserializeObject<WordpressTopic>(content);

            return newTopic;
        }

        public async Task<DateTime> GetLastDateSolidJoys()
        {
            var response = await _client.GetAsync("posts?page=1&per_page=1&author=1&order=desc&orderby=date&status=publish&categories=3&tags=57");

            var content = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var entries = JsonConvert.DeserializeObject<List<WordpressEntryResult>>(content);
                var last = entries.FirstOrDefault();
                return last != null ? last.Date : new DateTime();
            }
            else
            {
                Console.WriteLine($"Error retrieving posts from wordpress: {content}");
                return DateTime.MaxValue;
            }
        }

        public async Task<List<WordpressTopic>> GetTopics()
        {
            var page = 1;

            var result = new List<WordpressTopic>();

            List<WordpressTopic> entries = null;

            do
            {
                var response = await _client.GetAsync($"tags?page={page++}&per_page={PAGE_SIZE}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    entries = JsonConvert.DeserializeObject<List<WordpressTopic>>(content);
                    if (entries.Count > 0) result.AddRange(entries);
                }
                else
                {
                    throw new Exception($"Error retrieving topics from wordpress: {content}");
                }

            } while (entries != null && entries.Count == PAGE_SIZE);

            return result;
        }

        public async Task<List<WordpressAuthor>> GetAuthors()
        {
            var page = 1;

            var result = new List<WordpressAuthor>();

            List<WordpressAuthor> entries = null;

            do
            {
                var response = await _client.GetAsync($"users?page={page++}&per_page={PAGE_SIZE}");

                var content = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    entries = JsonConvert.DeserializeObject<List<WordpressAuthor>>(content);
                    if (entries.Count > 0) result.AddRange(entries);
                }
                else
                {
                    throw new Exception($"Error retrieving authors from wordpress: {content}");
                }

            } while (entries != null && entries.Count == PAGE_SIZE);

            return result;
        }

        public async Task<WordpressEntryResult> GetLastArticle()
        {
            var result = await _client.GetAsync($"posts?page=1&per_page=1&order=desc&categories=2");

            var content = await result.Content.ReadAsStringAsync();

            if (!result.IsSuccessStatusCode) throw new Exception($"Error searching articles...");

            var postEntries = JsonConvert.DeserializeObject<List<WordpressEntryResult>>(content);

            return postEntries.FirstOrDefault();
        }
    }
}
