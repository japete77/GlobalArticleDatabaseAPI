using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPITests.Builders;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace GlobalArticleDatabaseAPITests
{
    [Collection("WebAppCollection")]
    public class ArticleTests : IntegrationTestBase
    {
        private WebAppContext _webAppContext { get; }

        private readonly string text = "This a sample text";
        private readonly string textUpdated = "This a sample text updated";

        public ArticleTests(WebAppContext webAppContext)
        {
            _webAppContext = webAppContext;
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Article_GetNoNExisting_NotFound()
        {
            using var client = await _webAppContext.GetLoggedClient();
            using var httpResponse = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{ObjectId.GenerateNewId()}"
            );

            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.NotFound, "Found non existing article");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Article_CreateWithMissingFields_BadRequest()
        {
            using var client = await _webAppContext.GetLoggedClient();

            var article = new ArticleModelBuilder()
                .WithRandomValues()
                .WithAuthor(null)
                .Build();

            var httpResponse = await CallApiAsync(
                client.PostAsync,
                $"/api/v1/article",
                new CreateArticleRequest
                {
                    Article = article
                }
            );
            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest, "Article created with missing Author");

            article = new ArticleModelBuilder()
                .WithRandomValues()
                .WithDate(new DateTime())
                .Build();

            httpResponse = await CallApiAsync(
                client.PostAsync,
                $"/api/v1/article",
                new CreateArticleRequest
                {
                    Article = article
                }
            );

            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest, "Article created with missing Date");

            article = new ArticleModelBuilder()
                .WithRandomValues()
                .WithLanguage(null)
                .Build();

            httpResponse = await CallApiAsync(
                client.PostAsync,
                $"/api/v1/article",
                new CreateArticleRequest
                {
                    Article = article
                }
            );

            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest, "Article created with missing Language");

            article = new ArticleModelBuilder()
                .WithRandomValues()
                .WithOwner(null)
                .Build();

            httpResponse = await CallApiAsync(
                client.PostAsync,
                $"/api/v1/article",
                new CreateArticleRequest
                {
                    Article = article
                }
            );

            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest, "Article created with missing Owner");

            article = new ArticleModelBuilder()
                .WithRandomValues()
                .WithTitle(null)
                .Build();

            httpResponse = await CallApiAsync(
                client.PostAsync,
                $"/api/v1/article",
                new CreateArticleRequest
                {
                    Article = article
                }
            );

            Assert.True(httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest, "Article created with missing Title");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Article_CreateRetrieveUpdateDeleteWithNoTextImageTranslations_Success()
        {
            var article = new ArticleModelBuilder()
                .WithRandomValues()
                .Build();

            using var client = await _webAppContext.GetLoggedClient();

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

            Assert.True(dataCreate != null, "No data received");
            Assert.True(dataCreate.Article != null, "No article created");
            Assert.True(dataCreate.Article.Id != null, "No article Id created");
            Assert.True(dataCreate.Article.Author == article.Author, "Author mismatch");
            Assert.True(dataCreate.Article.Category == article.Category, "Category mismatch");
            Assert.True(dataCreate.Article.Date.Ticks == article.Date.Ticks, "Date mismatch");
            Assert.True(dataCreate.Article.Language == article.Language, "Language mismatch");
            Assert.True(dataCreate.Article.Owner == article.Owner, "Owner mismatch");
            Assert.True(dataCreate.Article.SourceLink == article.SourceLink, "SourceLink mismatch");
            Assert.True(dataCreate.Article.Subtitle == article.Subtitle, "Subtitle mismatch");
            Assert.True(dataCreate.Article.Summary == article.Summary, "Summary mismatch");
            Assert.True(dataCreate.Article.Title == article.Title, "Title mismatch");

            // Retrieve article
            using var httpResponseRetrieve = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseCreate.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseRetrieve.StatusCode}");

            var dataRetrieve = await GetResponse<Article>(httpResponseRetrieve.Content);

            Assert.True(dataRetrieve != null, "No data received");
            Assert.True(dataCreate.Article.Id == dataRetrieve.Id, "Id mismatch");
            Assert.True(dataCreate.Article.Author == dataRetrieve.Author, "Author mismatch");
            Assert.True(dataCreate.Article.Category == dataRetrieve.Category, "Category mismatch");
            Assert.True(DateEquals(dataCreate.Article.Date, dataRetrieve.Date), "Date mismatch");
            Assert.True(dataCreate.Article.Language == dataRetrieve.Language, "Language mismatch");
            Assert.True(dataCreate.Article.Owner == dataRetrieve.Owner, "Owner mismatch");
            Assert.True(dataCreate.Article.SourceLink == dataRetrieve.SourceLink, "SourceLink mismatch");
            Assert.True(dataCreate.Article.Subtitle == dataRetrieve.Subtitle, "Subtitle mismatch");
            Assert.True(dataCreate.Article.Summary == dataRetrieve.Summary, "Summary mismatch");
            Assert.True(dataCreate.Article.Title == dataRetrieve.Title, "Title mismatch");

            // Update article
            var updateArticle = new ArticleModelBuilder()
                .WithRandomValues()
                .WithId(dataRetrieve.Id)
                .Build();

            using var httpResponseUpdate = await CallApiAsync<UpdateArticleRequest>(
                client.PutAsync,
                $"/api/v1/article",
                new UpdateArticleRequest
                {
                    Article = updateArticle
                }
            );

            Assert.True(httpResponseUpdate.StatusCode == System.Net.HttpStatusCode.OK, $"Error in update article response: {httpResponseUpdate.StatusCode}");

            var dataUpdate = await GetResponse<UpdateArticleResponse>(httpResponseUpdate.Content);

            Assert.True(updateArticle != null, "No data received");
            Assert.True(updateArticle.Id == dataUpdate.Article.Id, "Id mismatch");
            Assert.True(updateArticle.Author == dataUpdate.Article.Author, "Author mismatch");
            Assert.True(updateArticle.Category == dataUpdate.Article.Category, "Category mismatch");
            Assert.True(DateEquals(updateArticle.Date, dataUpdate.Article.Date), "Date mismatch");
            Assert.True(updateArticle.Language == dataUpdate.Article.Language, "Language mismatch");
            Assert.True(updateArticle.Owner == dataUpdate.Article.Owner, "Owner mismatch");
            Assert.True(updateArticle.SourceLink == dataUpdate.Article.SourceLink, "SourceLink mismatch");
            Assert.True(updateArticle.Subtitle == dataUpdate.Article.Subtitle, "Subtitle mismatch");
            Assert.True(updateArticle.Summary == dataUpdate.Article.Summary, "Summary mismatch");
            Assert.True(updateArticle.Title == dataUpdate.Article.Title, "Title mismatch");

            // Delete
            using var httpResponseDelete = await CallApiAsync(
                client.DeleteAsync,
                $"/api/v1/article/{dataUpdate.Article.Id}"
            );

            Assert.True(httpResponseDelete.StatusCode == System.Net.HttpStatusCode.OK, $"Error in deleting article response: {httpResponseDelete.StatusCode}");

            // Check article deleted
            using var httpResponseRetrieveDeleted = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseRetrieveDeleted.StatusCode == System.Net.HttpStatusCode.NotFound, $"Error checking article 'not found' after deleting it");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Article_CreateWithTextImageUpdateDelete_Success()
        {
            var article = new ArticleModelBuilder()
                .WithRandomValues()
                .Build();

            using var client = await _webAppContext.GetLoggedClient();

            // Create article
            using var httpResponseCreate = await CallApiAsync<CreateArticleRequest>(
                client.PostAsync,
                $"/api/v1/article",
                new CreateArticleRequest
                {
                    Article = article,
                    Text = text
                }
            );

            Assert.True(httpResponseCreate.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseCreate.StatusCode}");

            var dataCreate = await GetResponse<CreateArticleResponse>(httpResponseCreate.Content);

            Assert.True(dataCreate != null, "No data received");
            Assert.True(dataCreate.Article != null, "No article created");
            Assert.True(dataCreate.Article.Id != null, "No article Id created");
            Assert.True(dataCreate.Article.HasText, "Text missing");
            Assert.True(!string.IsNullOrEmpty(dataCreate.Article.TextLink), "Text link missing");
            Assert.True(!string.IsNullOrEmpty(dataCreate.Article.ImageLink), "Image link missing");

            // Check text and image files exists
            HttpClient httpGet = new HttpClient();

            var getTextResponse = await httpGet.GetAsync(dataCreate.Article.TextLink);
            Assert.True(getTextResponse.StatusCode == System.Net.HttpStatusCode.OK, $"Error retrieving text file: {getTextResponse.StatusCode}");

            var getImageResponse = await httpGet.GetAsync(dataCreate.Article.ImageLink);
            Assert.True(getImageResponse.StatusCode == System.Net.HttpStatusCode.OK, $"Error retrieving image file: {getImageResponse.StatusCode}");

            // Update article text
            using var httpResponseUpdateText = await CallApiAsync<UpdateArticleTextRequest>(
                client.PutAsync,
                $"/api/v1/article/text",
                new UpdateArticleTextRequest
                {
                    Id = dataCreate.Article.Id,
                    Text = textUpdated,
                }
            );
            Assert.True(httpResponseUpdateText.StatusCode == System.Net.HttpStatusCode.OK, $"Error updating article text: {httpResponseUpdateText.StatusCode}");

            var getTextUpdatedResponse = await httpGet.GetAsync(dataCreate.Article.TextLink);
            Assert.True(getTextUpdatedResponse.StatusCode == System.Net.HttpStatusCode.OK, $"Error retrieving text file: {getTextUpdatedResponse.StatusCode}");
            
            var updatedTextContent =  await getTextUpdatedResponse.Content.ReadAsStringAsync();
            Assert.True(updatedTextContent == textUpdated, "Text not updated");

            // Delete
            using var httpResponseDelete = await CallApiAsync(
                client.DeleteAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseDelete.StatusCode == System.Net.HttpStatusCode.OK, $"Error in deleting article response: {httpResponseDelete.StatusCode}");

            // Check article deleted and text, images deleted from S3
            using var httpResponseRetrieveDeleted = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseRetrieveDeleted.StatusCode == System.Net.HttpStatusCode.NotFound, $"Error checking article 'Not Found' after deleting it");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Article_CreateWithTranslations_TranslationsIgnored()
        {
            var article = new ArticleModelBuilder()
                .WithRandomValues()
                .Build();

            article.Translations = new List<Translation>
            {
                new TranslationModelBuilder()
                    .WithRandomValues()
                    .Build()
            };

            using var client = await _webAppContext.GetLoggedClient();

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

            Assert.True(dataCreate != null, "No data received");
            Assert.True(dataCreate.Article != null, "No article created");
            Assert.True(dataCreate.Article.Translations == null || dataCreate.Article.Translations.Count == 0, "Translations not ignored");

            // Delete
            using var httpResponseDelete = await CallApiAsync(
                client.DeleteAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseDelete.StatusCode == System.Net.HttpStatusCode.OK, $"Error in deleting article response: {httpResponseDelete.StatusCode}");

        }

        private bool ArrayEquals(byte[] a1, byte[] b1)
        {
            int i;
            if (a1.Length == b1.Length)
            {
                i = 0;
                while (i < a1.Length && (a1[i] == b1[i])) //Earlier it was a1[i]!=b1[i]
                {
                    i++;
                }
                if (i == a1.Length)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
