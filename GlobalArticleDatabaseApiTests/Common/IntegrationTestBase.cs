using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseApiTests
{
    public class IntegrationTestBase
    {
        public async Task<HttpResponseMessage> CallApiAsync<T>(Func<string, HttpContent, Task<HttpResponseMessage>> function, string uri, T request)
        {
            var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            
            return await function.Invoke(uri, httpContent);
        }

        public async Task<HttpResponseMessage> CallApiAsync(Func<string, HttpContent, Task<HttpResponseMessage>> function, string uri)
        {
            var httpContent = new StringContent("", Encoding.UTF8, "application/json");

            return await function.Invoke(uri, httpContent);
        }

        public async Task<HttpResponseMessage> CallApiAsync(Func<string, Task<HttpResponseMessage>> function, string uri)
        {
            return await function.Invoke(uri);
        }

        public async Task<T> GetResponse<T>(HttpContent content)
        {
            var contentResponse = await content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(contentResponse);
        }
    }
}
