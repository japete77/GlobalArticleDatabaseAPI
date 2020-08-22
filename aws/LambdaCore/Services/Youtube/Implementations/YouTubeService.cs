using LambdaCore.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace LambdaCore.Services.Youtube.Implementations
{
    public class YouTubeService
    {
        private readonly HttpClient _client;
        public YouTubeService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
        }

        public async Task<GetItemsResponse> GetPlaylistItems(string playlistId, string youTubeChannelId, string youtubeKey, string pageToken)
        {
            var url = $"playlistItems?part=snippet&playlistId={playlistId}&maxResults=50&channelId={youTubeChannelId}&key={youtubeKey}";
            if (!string.IsNullOrEmpty(pageToken))
            {
                url += $"&pageToken={pageToken}";
            }

            var response = await _client.GetAsync(url);

            return JsonConvert.DeserializeObject<GetItemsResponse>(await response.Content.ReadAsStringAsync());
        }
    }
}
