using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
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
    public class PublicationV1Controller : ControllerBase
    {
        IPublicationService _publicationService { get; }

        public PublicationV1Controller(IPublicationService publicationService)
        {
            _publicationService = publicationService ?? throw new Exception(nameof(publicationService));
        }

        /// <summary>
        /// Create publication
        /// </summary>
        /// <param name="request">Request to create publication</param>
        [Route("publication")]
        [HttpPost]
        public async Task Create(CreatePublicationRequest request)
        {
            request.ValidateAndThrow();

            await _publicationService.Create(request);
        }

        /// <summary>
        /// Delete publication
        /// </summary>
        [Route("publication")]
        [HttpDelete]
        public async Task Delete([Required]string articleId, [Required]string language, [Required]string publisher)
        {
            await _publicationService.Delete(articleId, language, publisher);
        }
    }
}
