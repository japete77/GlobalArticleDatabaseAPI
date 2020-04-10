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
        private readonly string imageBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHDSURBVDhPY/z//z8DKYAJShMNoDZ8PNX++8MdIOPT9n////Ao9fd/u3P3clGFmC0vnzErUJyJQ1DIrgfIYAFiIPj14fbvN5eAjP8c/z/t+Pdu+/anm3e8OnxM2k/m1xtGoDgTpyhYIZKT2EQNxfw28tnpMfMxfNiz5832nQphNsx8jPymleL+W0TdF0KUITQwMrMzc4oxsnNyW7H9//GDi5dTMTcTKP794c5PF6f9eHIQogyh4eeLE89XWv56cYrf1vH32XNijnYc8vJA8V+vzn1/sO3n6/MQZVA/gADIBlFOOZef2z6x//7zn5Hx07GjQGFe3Qw2cSMmdkGIKoQN7KKG4n4bOGUj/63fxG1n+/3evVfzlwDF/359/vv9LaA9EGXo8fBj8RLOHz/5UpKFvL1/3H376/H/b/c2fjrb8+nCZIgCqJO4VUMZmVn/fPr0Z9FyFjNTTn19Vnl5Zm5uFjEGNgVQsDIysUFUMgAjDg6e9E68JSr/9fgJKB8bQGj48/XrUTmNiy4+UD4OgNDwdPqsfSxC73btgfJxAKiGvz9+nNI1OWfr8u/fP4gILgBNfP+BCr9/Z2RlZWKDeQ4rYGAAAI3bQ7yc8FrMAAAAAElFTkSuQmCC";
        private readonly string imageBase64Updated = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAGESURBVDhPY6A5YIRQQvxctvriQAbvB/Z7774ce/IEyPaRkeOVYv/G8QfIPn/r7aMXn4AMZiAGAldz6enpwt4mnDb8rG8ufjn6+bO1jFzMu68Ocbz3X33kZv39/df/J69/IDSoywsEeVoLu8z8z3iV9eyb/0IScV9+6QVazrtxc86OV+fufIWoBgImCAUEjMzszJxijOyc6s68TVZW8oK8irmZUY4ifnaKEiI8UEXIGn6+OPF8peWvF6f4bR1/nz0n5mjHIS8PFBfj+mqpxsbDyQZRxgKhQABkgyinnMvPbZ/Yf//5z8j46djRZfvfAJ0EVQAGCD8Ee9k9EM5NLuznOHRkIQuT2OfP7LeuKtr823b227cfvyHKgAAarBryAsqyQk8+8168eNGIg/Pcj++ywsJ1goJnZT6//PX7xftfP/8wnrv9GaIYBehwcHnIykHYbCxIDsYKFIRFGniEoBwcABFKQOD/l+kiHzeUQxBICghUMRFWjbDBjZXjrrgglEM1wMAAACDwgaAokUkqAAAAAElFTkSuQmCC";
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
                    Text = text,
                    ImageBase64 = imageBase64
                }
            );

            Assert.True(httpResponseCreate.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseCreate.StatusCode}");

            var dataCreate = await GetResponse<CreateArticleResponse>(httpResponseCreate.Content);

            Assert.True(dataCreate != null, "No data received");
            Assert.True(dataCreate.Article != null, "No article created");
            Assert.True(dataCreate.Article.Id != null, "No article Id created");
            Assert.True(dataCreate.Article.HasText, "Text missing");
            Assert.True(!string.IsNullOrEmpty(dataCreate.Article.TextLink), "Text link missing");
            Assert.True(dataCreate.Article.HasImage, "Image missing");
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

            // Update article image
            using var httpResponseUpdateImage = await CallApiAsync<UpdateArticleImageRequest>(
                client.PutAsync,
                $"/api/v1/article/image",
                new UpdateArticleImageRequest
                {
                    Id = dataCreate.Article.Id,
                    ImageBase64 = imageBase64Updated,
                }
            );

            Assert.True(httpResponseUpdateImage.StatusCode == System.Net.HttpStatusCode.OK, $"Error updating article text: {httpResponseUpdateImage.StatusCode}");

            var getImageUpdatedResponse = await httpGet.GetAsync(dataCreate.Article.ImageLink);
            Assert.True(getImageUpdatedResponse.StatusCode == System.Net.HttpStatusCode.OK, $"Error retrieving text file: {getImageUpdatedResponse.StatusCode}");
            
            byte[] remoteImage = await getImageUpdatedResponse.Content.ReadAsByteArrayAsync();
            byte[] updatedImageContent = Convert.FromBase64String(imageBase64Updated);

            Assert.True(ArrayEquals(remoteImage, updatedImageContent), "Image not updated");

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
