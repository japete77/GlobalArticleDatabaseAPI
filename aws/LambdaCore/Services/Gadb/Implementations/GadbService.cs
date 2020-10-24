using GlobalArticleDatabaseAPI.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace LambdaCore.Services.Gadb.Implementations
{
    public class GadbService
    {
        private readonly HttpClient _client;
        public GadbService()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri(Config.Settings.GadbApi);
        }

        public async Task Login(string username, string password)
        {
            var loginRequest = new LoginRequest
            {
                User = username,
                Password = password
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            using (var httpResponse = await _client.PostAsync("api/v1/auth/login", httpContent))
            {
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var contentResponse = await httpResponse.Content.ReadAsStringAsync();

                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(contentResponse);

                    // set authentication header
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                }
                else
                {
                    throw new Exception("Invalid user, password or token");
                }
            }
        }

        public async Task<Article> GetArticle(string id)
        {
            using (var httpResponse = await _client.GetAsync($"api/v1/article/{id}"))
            {
                var contentResponse = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<Article>(contentResponse);
                }
                else
                {
                    throw new Exception($"Error retrieving article {id}: {contentResponse}");
                }
            }
        }

        public async Task AddPublishDate(CreatePublicationRequest request)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            using (var httpResponse = await _client.PostAsync($"api/v1/publication", httpContent))
            {
                var contentResponse = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Error creating publication for article id {request.ArticleId}: {contentResponse}");
                }
            }
        }

        public async Task DeletePublishDate(string articleId, string language, string publisher)
        {
            using (var httpResponse = await _client.DeleteAsync($"api/v1/publication?articleId={articleId}&language={language}&publisher={publisher}"))
            {
                if (httpResponse.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception($"Error creating deleteing publication for article id {articleId} language {language} and publisher {publisher}");
                }
            }
        }
    }
}
