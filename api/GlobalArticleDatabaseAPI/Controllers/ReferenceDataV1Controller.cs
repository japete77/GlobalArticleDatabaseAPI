using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Services.ReferenceData.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace GlobalArticleDatabaseAPI.Controllers
{
    /// <summary>
    /// Reference data
    /// </summary>
    [Route("/api/v1/reference-data/")]
    [ApiController]
    [Produces("application/json")]
    public class ReferenceDataV1Controller : Controller
    {
        IReferenceDataService _referenceDataService { get; }

        public ReferenceDataV1Controller(IReferenceDataService referenceDataService)
        {
            _referenceDataService = referenceDataService ?? throw new ArgumentNullException(nameof(referenceDataService));
        }

        /// <summary>
        /// Create article
        /// </summary>
        [Route("authors")]
        [HttpGet]
        public async Task<GetListResponse> GetAuthors()
        {
            var result = await _referenceDataService.GetAuthors();

            return new GetListResponse
            {
                Items = result
            };
        }

        /// <summary>
        /// Create article
        /// </summary>
        [Route("categories")]
        [HttpGet]
        public async Task<GetListResponse> GetCategories()
        {
            var result = await _referenceDataService.GetCategories();

            return new GetListResponse
            {
                Items = result
            };
        }

        /// <summary>
        /// Create article
        /// </summary>
        [Route("topics")]
        [HttpGet]
        public async Task<GetListResponse> GetTopics()
        {
            var result = await _referenceDataService.GetTopics();

            return new GetListResponse
            {
                Items = result
            };
        }

        /// <summary>
        /// Create article
        /// </summary>
        [Route("owners")]
        [HttpGet]
        public async Task<GetListResponse> GetOwners()
        {
            var result = await _referenceDataService.GetOwners();

            return new GetListResponse
            {
                Items = result
            };
        }
    }
}
