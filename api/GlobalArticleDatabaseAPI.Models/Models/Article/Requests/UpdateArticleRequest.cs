using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Article entry
    /// </summary>
    public class UpdateArticleRequest : BaseModel<UpdateArticleRequest, UpdateArticleRequestValidator>
    {
        /// <summary>
        /// Article metadata
        /// </summary>
        public Article Article { get; set; }
    }

    public class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
    {
        public UpdateArticleRequestValidator()
        {
            RuleFor(model => model.Article).NotEmpty();
            RuleFor(model => model.Article.Id).NotEmpty();
            RuleFor(model => model.Article.Author).NotEmpty();
            RuleFor(model => model.Article.Date).NotEmpty();
            RuleFor(model => model.Article.Language).NotEmpty();
            RuleFor(model => model.Article.Owner).NotEmpty();
            RuleFor(model => model.Article.Title).NotEmpty();
        }
    }

}
