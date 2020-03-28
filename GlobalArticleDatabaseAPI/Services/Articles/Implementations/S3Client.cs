using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Config.Interfaces;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Services.Articles.Implementations
{
    public class S3Client : IS3Client
    {
        ISettings _settings { get; }

        public S3Client(ISettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public IAmazonS3 GetClient()
        {
            IAmazonS3 clientS3 = null;

            if (string.IsNullOrEmpty(_settings.AWSAccessKey) ||
                string.IsNullOrEmpty(_settings.AWSSecretKey))
            {
                clientS3 = new AmazonS3Client(RegionEndpoint.EUWest1);
            }
            else
            {
                var credentials = new BasicAWSCredentials(_settings.AWSAccessKey, _settings.AWSSecretKey);
                clientS3 = new AmazonS3Client(
                    credentials,
                    RegionEndpoint.EUWest1
                );
            }

            return clientS3;
        }
    }
}
