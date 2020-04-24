using GlobalArticleDatabaseAPI.Helpers;
using GlobalArticleDatabaseAPI.Models;
using GlobalArticleDatabaseAPI.Models.Workspace;
using GlobalArticleDatabaseAPI.Models.Workspace.Responses;
using GlobalArticleDatabaseAPI.Services.Articles.Interfaces;
using GlobalArticleDatabaseAPI.Services.Authentication.Implementations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GlobalArticleDatabaseAPI.Controllers
{
    /// <summary>
    /// Articles management
    /// </summary>
    [Route("/api/v1/")]
    [ApiController]
    [Produces("application/json")]
    public class WorkspaceV1Controller : ControllerBase
    {
        IHttpContextAccessor _httpContext { get; }

        IArticleService _articlesService { get; }

        public WorkspaceV1Controller(IHttpContextAccessor httpContext, IArticleService articlesService)
        {
            _httpContext = httpContext ?? throw new Exception(nameof(httpContext));
            _articlesService = articlesService ?? throw new Exception(nameof(articlesService));
        }

        /// <summary>
        /// Get workspace
        /// </summary>
        [Route("workspace")]
        [HttpGet]
        public async Task<WorkspaceResponse> GetWorkspace()
        {
            var token = JwtRetriever.GetUserToken(_httpContext.HttpContext);

            var claimsHelper = new ClaimsHelper(token.Claims);

            List<WorkspaceEntry> workspaceEntries = new List<WorkspaceEntry>
            {
                new WorkspaceEntry
                {
                    Name = "All Asigned To Me",
                    Count = await _articlesService.SearchCount(new ArticleFilter { ReviewedBy = claimsHelper.UserName }),
                    Reviewer = claimsHelper.UserName                }
            };

            var statuses = TranslationStatus.GetTraslationStatus();

            foreach (var status in statuses)
            {
                if (status == TranslationStatus.PUBLISHED) continue;

                workspaceEntries.Add(new WorkspaceEntry
                {
                    Name = status,
                    Count = await _articlesService.SearchCount(new ArticleFilter { ReviewedBy = claimsHelper.UserName, Status = status }),
                    Reviewer = claimsHelper.UserName,
                    Status = status
                });
            }

            return new WorkspaceResponse
            {
                WorkspaceEntries = workspaceEntries
            };
        }
    }
}
