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
        string Secret { get; }
        string DefaultEmail { get; }
        bool SmtpEnabled { get; }
        string SmtpServer { get; }
        int SmtpPort { get; }
        string SmtpUserName { get; }
        string SmtpPassword { get; }
        string SmtpFrom { get; }
        string SmtpFromName { get; }
        string ResetPasswordUrl { get; }
        string SupportUrl { get; }

    }
}
