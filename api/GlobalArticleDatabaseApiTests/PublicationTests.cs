using Core.Exceptions;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPITests.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GlobalArticleDatabaseAPITests
{
    [Collection("WebAppCollection")]
    public class PublicationTests : IntegrationTestBase
    {
        private WebAppContext _webAppContext { get; }

        public PublicationTests(WebAppContext webAppContext)
        {
            _webAppContext = webAppContext;
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Publication_CreateDelete_Success()
        {
            var article = new ArticleModelBuilder()
                .WithRandomValues()
                .Build();

            using var client = _webAppContext.GetAnonymousClient();

            // Create article
            using var httpResponseCreate = await CallApiAsync<CreateArticleRequest>(
                client.PostAsync,
                $"/api/v1/article",
                new CreateArticleRequest
                {
                    Article = article
                }
            );

            Assert.True(httpResponseCreate.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseCreate.StatusCode}");

            var dataCreate = await GetResponse<CreateArticleResponse>(httpResponseCreate.Content);

            var translation = new TranslationModelBuilder().WithRandomValues().Build();

            // Create translation
            using var httpResponseCreateTranslation = await CallApiAsync<CreateTranslationRequest>(
                client.PostAsync,
                $"/api/v1/translation",
                new CreateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Text = "Prueba",
                    Translation = translation
                }
            );

            Assert.True(httpResponseCreateTranslation.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create translation response: {httpResponseCreateTranslation.StatusCode}");

            var publication = new PublicationModelBuilder().WithRandomValues().Build();

            // Create publication
            using var httpResponseCreatePublication = await CallApiAsync<CreatePublicationRequest>(
                client.PostAsync,
                $"/api/v1/publication",
                new CreatePublicationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Language = translation.Language,
                    Publication = publication
                }
            );

            Assert.True(httpResponseCreatePublication.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create publication response: {httpResponseCreatePublication.StatusCode}");

            // Retrieve article
            using var httpResponseRetrieve = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseRetrieve.StatusCode == System.Net.HttpStatusCode.OK, $"Error in retrieve article response: {httpResponseRetrieve.StatusCode}");

            var dataArticle = await GetResponse<Article>(httpResponseRetrieve.Content);

            Assert.True(dataArticle.Translations != null && dataArticle.Translations.Count > 0, "No data received");

            Assert.True(DateEquals(dataArticle.Translations.First().Date, translation.Date), "Date mismatch");
            Assert.True(dataArticle.Translations.First().Language == translation.Language , "Language mismatch");
            Assert.True(dataArticle.Translations.First().ReviewedBy == translation.ReviewedBy, "ReviewedBy mismatch");
            Assert.True(dataArticle.Translations.First().Status == translation.Status, "Status mismatch");
            Assert.True(dataArticle.Translations.First().Subtitle == translation.Subtitle, "Subtitle mismatch");
            Assert.True(dataArticle.Translations.First().Summary == translation.Summary, "Summary mismatch");
            Assert.True(dataArticle.Translations.First().Title == translation.Title, "Title mismatch");
            Assert.True(dataArticle.Translations.First().TranslatedBy == translation.TranslatedBy, "TranslatedBy mismatch");

            Assert.True(DateEquals(dataArticle.Translations.First().Publications.First().Date, publication.Date), "Publication Date mismatch");
            Assert.True(dataArticle.Translations.First().Publications.First().Link == publication.Link, "Publication Link mismatch");
            Assert.True(dataArticle.Translations.First().Publications.First().Publisher == publication.Publisher, "Publication Publisher mismatch");

            // Delete
            using var httpResponseDelete = await CallApiAsync(
                client.DeleteAsync,
                $"/api/v1/publication?articleId={dataArticle.Id}&language={translation.Language}&publisher={publication.Publisher}"
            );

            Assert.True(httpResponseDelete.StatusCode == System.Net.HttpStatusCode.OK, $"Error in deleting publication response: {httpResponseDelete.StatusCode}");

            // Check article
            using var httpResponseRetrieveCheck = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseRetrieveCheck.StatusCode == System.Net.HttpStatusCode.OK, $"Error in retrieve article response: {httpResponseRetrieveCheck.StatusCode}");

            var dataArticleCheck = await GetResponse<Article>(httpResponseRetrieveCheck.Content);

            Assert.True(dataArticleCheck.Translations.Count > 0, "Transations missing");
            Assert.True(dataArticleCheck.Translations.Where(w => w.Language == article.Language && w.Publications.Any(a => a.Publisher == publication.Publisher)).Count() == 0, "Publication not deleted");
        }
    }
}
