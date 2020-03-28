using Xunit;

namespace GlobalArticleDatabaseApiTests
{
    [CollectionDefinition("WebAppCollection")]
    public class WebAppCollection : ICollectionFixture<WebAppContext>
    {
    }
}
