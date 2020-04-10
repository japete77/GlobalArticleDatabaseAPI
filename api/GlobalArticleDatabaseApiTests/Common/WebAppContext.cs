using Core.Exceptions;
using GlobalArticleDatabaseAPI;
using GlobalArticleDatabaseAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPITests
{
    public class WebAppContext : IDisposable
    {
        private WebApplicationFactory<Startup> _factory;
        private MongoDbRunner _dbRunner;

        public HttpClient GetAnonymousClient()
        {
            return GetFactory().CreateClient();
        }
        public string GetDbServer()
        {
            return _dbRunner.ConnectionString;
        }

        public async Task<HttpClient> GetLoggedClient(string username = "admin", string password = "admin")
        {
            var client = GetFactory().CreateClient();

            var loginRequest = new LoginRequest
            {
                User = username,
                Password = password
            };

            var httpContent = new StringContent(JsonConvert.SerializeObject(loginRequest), Encoding.UTF8, "application/json");

            using (var httpResponse = await client.PostAsync("api/v1/auth/login", httpContent))
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

        public T GetService<T>()
        {
            return Startup.GetService<T>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (_factory != null)
            {
                _factory.Dispose();
                _factory = null;
            }

            if (_dbRunner != null)
            {
                _dbRunner.Dispose();
                _dbRunner = null;
            }
        }
        private WebApplicationFactory<Startup> GetFactory()
        {
            if (_factory == null)
            {
                // run mongo db
                _dbRunner = MongoDbRunner.Start();

                var projectDir = Directory.GetCurrentDirectory();
                var configPath = Path.Combine(projectDir, "appsettings.json");

                // amend mongodb connection string
                string json = File.ReadAllText(configPath);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                jsonObj["Server"] = _dbRunner.ConnectionString;
                jsonObj["S3Url"] = "https://gadb-articles-test.s3-eu-west-1.amazonaws.com";
                jsonObj["AWS"]["Bucket"] = "gadb-articles-test";
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, output);

                // start up web app context
                _factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(
                    builder =>
                    {
                        builder.ConfigureAppConfiguration((context, conf) =>
                        {
                            conf.AddJsonFile(configPath);
                        });
                    });
            }

            return _factory;
        }
    }
}
