using Core.Exceptions;
using GlobalArticleDatabaseAPI.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DesiringGodArticlesCrawler
{
    public class ClientHelper
    {
        public async Task<HttpClient> GetLoggedClient(string username = "admin", string password = "admin")
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://ig24hiba4k.execute-api.eu-west-1.amazonaws.com/Prod/api/v1/");

            var loginRequest = new LoginRequest
            {
                User = username,
                Password = password
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            using (var httpResponse = await client.PostAsync("auth/login", httpContent))
            {
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    var contentResponse = await httpResponse.Content.ReadAsStringAsync();

                    var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(contentResponse);

                    // set authentication header
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);
                }
                else
                {
                    throw new AuthenticationException(ExceptionCodes.IDENTITY_INVALID_USER_PASSWORD, "Invalid user, password or token", null, StatusCodes.Status401Unauthorized);
                }
            }

            return client;
        }
    }
}
