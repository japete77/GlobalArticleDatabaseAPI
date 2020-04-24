using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
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
    public class TranslationV1Controller : ControllerBase
    {
        ITranslationService _translationService { get; }

        public TranslationV1Controller(ITranslationService translationService)
        {
            _translationService = translationService ?? throw new Exception(nameof(translationService));
        }

        /// <summary>
        /// Create translation
        /// </summary>
        /// <param name="request">Request to create translation</param>
        [Route("translation")]
        [HttpPost]
        public async Task Create(CreateTranslationRequest request)
        {
            request.ValidateAndThrow();

            await _translationService.Create(request);
        }

        /// <summary>
        /// Update translation
        /// </summary>
        /// <param name="request">Request to update translation</param>
        [Route("translation")]
        [HttpPut]
        public async Task Update(UpdateTranslationRequest request)
        {
            request.ValidateAndThrow();

            await _translationService.Update(request);
        }

        /// <summary>
        /// Update translation text
        /// </summary>
        /// <param name="request">Request to update translation</param>
        [Route("translation/text")]
        [HttpPut]
        public async Task UpdateText(UpdateTranslationTextRequest request)
        {
            request.ValidateAndThrow();

            await _translationService.UpdateText(request);
        }

        /// <summary>
        /// Delete translation
        /// </summary>
        [Route("translation")]
        [HttpDelete]
        public async Task Delete([Required]string articleId, [Required]string language)
        {
            await _translationService.Delete(articleId, language);
        }

        /// <summary>
        /// Get status list
        /// </summary>
        [Route("translation/status")]
        [HttpGet]
        public List<string> GetStatus()
        {
            return TranslationStatus.GetTraslationStatus();
        }
    }
}
