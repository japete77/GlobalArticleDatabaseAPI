using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
using GlobalArticleDatabaseAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Controllers
{
    /// <summary>
    /// Articles management
    /// </summary>
    [Route("/api/v1/")]
    [ApiController]
    [Produces("application/json")]
    public class ArticleV1Controller : ControllerBase
    {
        IArticleService _articlesService { get; }

        public ArticleV1Controller(IArticleService articlesService)
        {
            _articlesService = articlesService ?? throw new Exception(nameof(articlesService));
        }

        /// <summary>
        /// Create article
        /// </summary>
        [Route("article")]
        [HttpPost]
        public async Task<CreateArticleResponse> Create(CreateArticleRequest request)
        {
            request.ValidateAndThrow();

            var result = await _articlesService.Create(request);

            return new CreateArticleResponse
            {
                Article = result
            };
        }

        /// <summary>
        /// Update article
        /// </summary>
        [Route("article")]
        [HttpPut]
        public async Task<UpdateArticleResponse> Update(UpdateArticleRequest request)
        {
            request.ValidateAndThrow();

            var result = await _articlesService.Update(request);

            return new UpdateArticleResponse
            {
                Article = result
            };
        }

        /// <summary>
        /// Update article text
        /// </summary>
        [Route("article/{id}/text")]
        [HttpGet]
        public async Task<string> GetText(string id, string language)
        {
            return await _articlesService.GetText(id, language);
        }

        /// <summary>
        /// Update article text
        /// </summary>
        [Route("article/text")]
        [HttpPut]
        public async Task UpdateText(UpdateArticleTextRequest request)
        {
            request.ValidateAndThrow();

            await _articlesService.UpdateText(request);
        }

        /// <summary>
        /// Get article
        /// </summary>
        [Route("article/{id}")]
        [HttpGet]
        public async Task<Article> Get([Required]string id)
        {
            return await _articlesService.Get(id);
        }

        /// <summary>
        /// Search articles
        /// </summary>
        [Route("articles")]
        [HttpGet]
        public async Task<ArticleSearchResponse> Search([FromQuery]ArticleFilter filter, [Required]int page, [Required]int pageSize)
        {
            return await _articlesService.Search(filter, page, pageSize);
        }

        /// <summary>
        /// Delete article
        /// </summary>
        /// <param name="id">Article unique id</param>
        [Route("article/{id}")]
        [HttpDelete]
        public async Task Delete([Required]string id)
        {
            await _articlesService.Delete(id);
        }
    }
}
