namespace Config.Interfaces
{
    public interface ISettings
    {
        string Server { get; }
        string Database { get; }
        string S3Url { get; }
        string AWSAccessKey { get; }
        string AWSSecretKey { get; }
        string AWSBucket { get; }
        int AWSLinkExpireInSecs { get; }
    }
}
