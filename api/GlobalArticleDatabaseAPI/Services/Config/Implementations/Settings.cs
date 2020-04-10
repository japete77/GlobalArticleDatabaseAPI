using Config.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace Config.Implementations
{
    public class Settings : ISettings
    {
        private IConfiguration _configuration { get; }
        public Settings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string Get(string name) => Environment.ExpandEnvironmentVariables(_configuration[name]);

        public string Server { get { return Get("Server"); } }
        public string Database { get { return Get("Database"); } }
        public string S3Url { get { return Get("S3Url"); } }
        public string AWSAccessKey { get { return Get("AWS:AccessKey"); } }
        public string AWSSecretKey { get { return Get("AWS:SecretKey"); } }
        public string AWSBucket { get { return Get("AWS:Bucket"); } }
        public int AWSLinkExpireInSecs { get { return Int32.Parse(Get("AWS:LinkExpireInSecs")); } }
        public string Secret { get { return Get("Secret"); } }
        public string DefaultEmail { get { return Get("DefaultEmail"); } }
    }
}
