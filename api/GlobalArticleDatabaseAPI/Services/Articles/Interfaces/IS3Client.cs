using Amazon.S3;

namespace GlobalArticleDatabaseAPI.Services.Articles.Interfaces
{
    public interface IS3Client
    {
        IAmazonS3 GetClient();
    }
}
