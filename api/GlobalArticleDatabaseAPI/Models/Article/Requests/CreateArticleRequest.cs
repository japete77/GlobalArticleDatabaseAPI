using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Article entry
    /// </summary>
    public class CreateArticleRequest : BaseModel<CreateArticleRequest, CreateArticleRequestValidator>
    {
        /// <summary>
        /// Article metadata
        /// </summary>
        public Article Article { get; set; }

        /// <summary>
        /// Article text content
        /// </summary>
        public string Text { get; set; }
    }


    public class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
    {
        public CreateArticleRequestValidator()
        {
            RuleFor(model => model.Article).NotEmpty();
            RuleFor(model => model.Article.Author).NotEmpty();
            RuleFor(model => model.Article.Date).NotEmpty();
            RuleFor(model => model.Article.Language).NotEmpty();
            RuleFor(model => model.Article.Owner).NotEmpty();
            RuleFor(model => model.Article.Title).NotEmpty();
        }
    }
}
