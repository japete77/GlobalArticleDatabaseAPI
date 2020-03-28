using GlobalArticleDatabase;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Mongo2Go;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net.Http;

namespace GlobalArticleDatabaseApiTests
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

        public T GetService<T>()
        {
            return Startup.GetService<T>();
        }

        public void Dispose()
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
