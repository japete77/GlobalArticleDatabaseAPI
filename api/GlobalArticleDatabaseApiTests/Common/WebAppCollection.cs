using Xunit;

namespace GlobalArticleDatabaseAPITests
{
    [CollectionDefinition("WebAppCollection")]
    public class WebAppCollection : ICollectionFixture<WebAppContext>
    {
    }
}
