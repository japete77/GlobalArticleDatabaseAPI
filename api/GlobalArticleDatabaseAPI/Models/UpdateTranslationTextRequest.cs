using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Create new translation request
    /// </summary>
    public class UpdateTranslationTextRequest : BaseModel<UpdateTranslationTextRequest, UpdateTranslationTextRequestValidator>
    {
        /// <summary>
        /// Article unique id
        /// </summary>
        public string ArticleId { get; set; }

        public string Text { get; set; }

        public string Language { get; set; }
    }

    public class UpdateTranslationTextRequestValidator : AbstractValidator<UpdateTranslationTextRequest>
    {
        public UpdateTranslationTextRequestValidator()
        {
            RuleFor(model => model.ArticleId).NotEmpty();
            RuleFor(model => model.Text).NotEmpty();
            RuleFor(model => model.Language).NotEmpty();
        }
    }
}
