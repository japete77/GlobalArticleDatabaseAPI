using GlobalArticleDatabase.Services.Articles.Interfaces;
using GlobalArticleDatabaseAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace GlobalArticleDatabase.Controllers
{
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
        /// Create articles
        /// </summary>
        [Route("articles")]
        [HttpPost]
        public async Task<CreateArticlesResponse> Create(CreateArticlesRequest request)
        {
            var result = await _articlesService.Create(request.Articles);

            return new CreateArticlesResponse
            {
                Articles = result
            };
        }

        /// <summary>
        /// Get article
        /// </summary>
        [Route("article/{id}")]
        [HttpGet]
        public async Task<Article> Get(string id)
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
        public async Task Delete(string id)
        {
            await _articlesService.Delete(id);
        }
    }
}
