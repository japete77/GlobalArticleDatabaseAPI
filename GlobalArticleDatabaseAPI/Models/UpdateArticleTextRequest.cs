using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Article text update request
    /// </summary>
    public class UpdateArticleTextRequest : BaseModel<UpdateArticleTextRequest, UpdateArticleTextRequestValidator>
    {
        /// <summary>
        /// Article unique identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Article text content
        /// </summary>
        public string Text { get; set; }
    }


    public class UpdateArticleTextRequestValidator : AbstractValidator<UpdateArticleTextRequest>
    {
        public UpdateArticleTextRequestValidator()
        {
            RuleFor(model => model.Id).NotEmpty();
            RuleFor(model => model.Text).NotEmpty();
        }
    }
}
