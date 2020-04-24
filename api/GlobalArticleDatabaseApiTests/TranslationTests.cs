using Core.Exceptions;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPITests.Builders;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace GlobalArticleDatabaseAPITests
{
    [Collection("WebAppCollection")]
    public class TranslationTests : IntegrationTestBase
    {
        private WebAppContext _webAppContext { get; }

        public TranslationTests(WebAppContext webAppContext)
        {
            _webAppContext = webAppContext;
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Translation_CreateUpdateDelete_Success()
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

            Assert.True(httpResponseCreateTranslation.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseCreateTranslation.StatusCode}");

            // Retrieve article
            using var httpResponseRetrieve = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseRetrieve.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create translation response: {httpResponseRetrieve.StatusCode}");

            var dataArticle = await GetResponse<Article>(httpResponseRetrieve.Content);

            Assert.True(dataArticle.Translations != null && dataArticle.Translations.Count > 0, "No data received");

            Assert.True(DateEquals(dataArticle.Translations.First().Date, translation.Date), "Date mismatch");
            Assert.True(dataArticle.Translations.First().Language == translation.Language, "Language mismatch");
            Assert.True(dataArticle.Translations.First().ReviewedBy == translation.ReviewedBy, "ReviewedBy mismatch");
            Assert.True(dataArticle.Translations.First().Status == translation.Status, "Status mismatch");
            Assert.True(dataArticle.Translations.First().Subtitle == translation.Subtitle, "Subtitle mismatch");
            Assert.True(dataArticle.Translations.First().Summary == translation.Summary, "Summary mismatch");
            Assert.True(dataArticle.Translations.First().Title == translation.Title, "Title mismatch");
            Assert.True(dataArticle.Translations.First().TranslatedBy == translation.TranslatedBy, "TranslatedBy mismatch");

            HttpClient httpGet = new HttpClient();
            var getTranslationTextResponse = await httpGet.GetAsync(dataArticle.Translations.Where(w => w.Language == translation.Language).First().TextLink);
            Assert.True(getTranslationTextResponse.StatusCode == System.Net.HttpStatusCode.OK, $"Error retrieving translation text file: {getTranslationTextResponse.StatusCode}");

            // Delete
            using var httpResponseDelete = await CallApiAsync(
                client.DeleteAsync,
                $"/api/v1/translation?articleId={dataArticle.Id}&language={translation.Language}"
            );

            Assert.True(httpResponseDelete.StatusCode == System.Net.HttpStatusCode.OK, $"Error in deleting article response: {httpResponseCreate.StatusCode}");

            // Check article
            using var httpResponseRetrieveCheck = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseRetrieveCheck.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseRetrieveCheck.StatusCode}");

            var dataArticleCheck = await GetResponse<Article>(httpResponseRetrieveCheck.Content);

            Assert.True(dataArticle.Translations.Where(w => w.Language == article.Language).Count() == 0, "Translation not deleted");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Translation_CreateDuplicated_ErrorRaised()
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

            var translation = new TranslationModelBuilder().WithRandomValues().Build();

            // Create translation
            using var httpResponseCreateTranslation = await CallApiAsync<CreateTranslationRequest>(
                client.PostAsync,
                $"/api/v1/translation",
                new CreateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Translation = translation
                }
            );

            Assert.True(httpResponseCreateTranslation.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseCreateTranslation.StatusCode}");

            // Create duplicated translation
            using var httpResponseCreateTranslationDuplicated = await CallApiAsync<CreateTranslationRequest>(
                client.PostAsync,
                $"/api/v1/translation",
                new CreateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Translation = translation
                }
            );

            Assert.True(httpResponseCreateTranslationDuplicated.StatusCode == System.Net.HttpStatusCode.BadRequest, $"Duplicated translation error not raised");

            var exception = await GetResponse<dynamic>(httpResponseCreateTranslationDuplicated.Content);
            Assert.True(exception.error.code == ExceptionCodes.TRANSLATION_ALREADY_EXISTS, "Invalid error code returned for duplicated translation");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Translation_CreateMultipleUpdateDelete_Success()
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
                    Text = "In ENglish"
                }
            );

            Assert.True(httpResponseCreate.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseCreate.StatusCode}");

            var dataCreate = await GetResponse<CreateArticleResponse>(httpResponseCreate.Content);

            var translationES = new TranslationModelBuilder()
                .WithRandomValues()
                .WithLanguage("es")
                .Build();

            // Create translation ES
            using var httpResponseCreateTranslationES = await CallApiAsync<CreateTranslationRequest>(
                client.PostAsync,
                $"/api/v1/translation",
                new CreateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Translation = translationES,
                    Text = "En español"
                }
            );

            Assert.True(httpResponseCreateTranslationES.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create translation response: {httpResponseCreateTranslationES.StatusCode}");

            var translationFR = new TranslationModelBuilder()
                .WithRandomValues()
                .WithLanguage("fr")
                .Build();

            // Create translation FR
            using var httpResponseCreateTranslationFR = await CallApiAsync<CreateTranslationRequest>(
                client.PostAsync,
                $"/api/v1/translation",
                new CreateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Translation = translationFR,
                    Text = "Fraçous"
                }
            );

            Assert.True(httpResponseCreateTranslationFR.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create translation response: {httpResponseCreateTranslationFR.StatusCode}");

            // Update translation FR
            var translationUpdateFR = new TranslationModelBuilder()
                .WithRandomValues()
                .WithLanguage("fr")
                .WithHasText(false) // To check that has text is not updated...
                .WithTextLink(null) // To check that link is not updated...
                .Build();

            using var httpResponseUpdateTranslationFR = await CallApiAsync<CreateTranslationRequest>(
                client.PutAsync,
                $"/api/v1/translation",
                new CreateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Translation = translationUpdateFR,
                    Text = "Fraçous 2"
                }
            );

            Assert.True(httpResponseUpdateTranslationFR.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create translation FR response: {httpResponseUpdateTranslationFR.StatusCode}");

            // Retrieve article and check fr translation update
            using var httpResponseRetrieve = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseCreate.StatusCode == System.Net.HttpStatusCode.OK, $"Error in translarion update FR response: {httpResponseRetrieve.StatusCode}");

            var dataRetrieve = await GetResponse<Article>(httpResponseRetrieve.Content);

            var dataFR = dataRetrieve.Translations.Where(w => w.Language == "fr").First();
            Assert.True(DateEquals(dataFR.Date, translationUpdateFR.Date), "Date mismatch");
            Assert.True(dataFR.HasText != translationUpdateFR.HasText, "HasText mismatch");
            Assert.True(dataFR.Language == translationUpdateFR.Language, "Language mismatch");
            Assert.True(dataFR.ReviewedBy == translationUpdateFR.ReviewedBy, "ReviewedBy mismatch");
            Assert.True(dataFR.Status == translationUpdateFR.Status, "Status mismatch");
            Assert.True(dataFR.Subtitle == translationUpdateFR.Subtitle, "Subtitle mismatch");
            Assert.True(dataFR.Summary == translationUpdateFR.Summary, "Summary mismatch");
            Assert.True(dataFR.TextLink != translationUpdateFR.TextLink, "TextLink mismatch");
            Assert.True(dataFR.Title == translationUpdateFR.Title, "Title mismatch");
            Assert.True(dataFR.TranslatedBy == translationUpdateFR.TranslatedBy, "TranslatedBy mismatch");

            // Update Text
            using var httpResponseUpdateTranslationTextFR = await CallApiAsync<UpdateTranslationTextRequest>(
                client.PutAsync,
                $"/api/v1/translation/text",
                new UpdateTranslationTextRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Language = "fr",
                    Text = "Updated"
                }
            );

            Assert.True(httpResponseUpdateTranslationTextFR.StatusCode == System.Net.HttpStatusCode.OK, $"Error in update translation text FR response: {httpResponseUpdateTranslationTextFR.StatusCode}");

            HttpClient httpGet = new HttpClient();
            var getTranslationTextFrResponse = await httpGet.GetAsync(dataFR.TextLink);
            Assert.True(getTranslationTextFrResponse.StatusCode == System.Net.HttpStatusCode.OK, $"Error retrieving translation text file: {getTranslationTextFrResponse.StatusCode}");

            var updatedTranslationTextFR = await getTranslationTextFrResponse.Content.ReadAsStringAsync();
            Assert.True(updatedTranslationTextFR == "Updated", "Translation text not updated");

            // Delete
            using var httpResponseDelete = await CallApiAsync(
                client.DeleteAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseDelete.StatusCode == System.Net.HttpStatusCode.OK, $"Error in deleting article response: {httpResponseCreate.StatusCode}");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Translation_UpdateNoExistingTranslation_NotFoundRaised()
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

            var translation = new TranslationModelBuilder().WithRandomValues().Build();

            // Update non existing translation
            using var httpResponseUpdateTranslation = await CallApiAsync<UpdateTranslationRequest>(
                client.PutAsync,
                $"/api/v1/translation",
                new UpdateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Translation = translation
                }
            );

            Assert.True(httpResponseUpdateTranslation.StatusCode == System.Net.HttpStatusCode.NotFound, $"Update error 'Not Found' not raised: {httpResponseUpdateTranslation.StatusCode}");
        }

        [Fact]
        [Trait("Category", "IntegrationTest")]
        public async Task Translation_CreateMultipleSearchByReviewer_Success()
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
                    Text = "In ENglish"
                }
            );

            Assert.True(httpResponseCreate.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create article response: {httpResponseCreate.StatusCode}");

            var dataCreate = await GetResponse<CreateArticleResponse>(httpResponseCreate.Content);

            var translationES = new TranslationModelBuilder()
                .WithRandomValues()
                .WithLanguage("es")
                .Build();

            // Create translation ES
            using var httpResponseCreateTranslationES = await CallApiAsync<CreateTranslationRequest>(
                client.PostAsync,
                $"/api/v1/translation",
                new CreateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Translation = translationES,
                    Text = "En español"
                }
            );

            Assert.True(httpResponseCreateTranslationES.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create translation response: {httpResponseCreateTranslationES.StatusCode}");

            var translationFR = new TranslationModelBuilder()
                .WithRandomValues()
                .WithLanguage("fr")
                .Build();

            // Create translation FR
            using var httpResponseCreateTranslationFR = await CallApiAsync<CreateTranslationRequest>(
                client.PostAsync,
                $"/api/v1/translation",
                new CreateTranslationRequest
                {
                    ArticleId = dataCreate.Article.Id,
                    Translation = translationFR,
                    Text = "Fraçous"
                }
            );

            Assert.True(httpResponseCreateTranslationFR.StatusCode == System.Net.HttpStatusCode.OK, $"Error in create translation response: {httpResponseCreateTranslationFR.StatusCode}");

            // Search by reviewed by
            using var httpResponseSearch = await CallApiAsync(
                client.GetAsync,
                $"/api/v1/articles?filter.reviewedBy={HttpUtility.UrlEncode(translationES.ReviewedBy)}&page=1&pageCount=10"
            );

            var searchResult = await GetResponse<ArticleSearchResponse>(httpResponseSearch.Content);

            Assert.True(searchResult.Total == 1, "Not found articles");
            Assert.True(searchResult.Articles != null && searchResult.Articles.Count == 1, "Not found articles");
            Assert.True(searchResult.Articles[0].Translations != null &&
                        searchResult.Articles[0].Translations.Where(w => w.ReviewedBy == translationES.ReviewedBy).Count() == 1, "Not found articles");

            // Delete
            using var httpResponseDelete = await CallApiAsync(
                client.DeleteAsync,
                $"/api/v1/article/{dataCreate.Article.Id}"
            );

            Assert.True(httpResponseDelete.StatusCode == System.Net.HttpStatusCode.OK, $"Error in deleting article response: {httpResponseCreate.StatusCode}");
        }
    }
}
