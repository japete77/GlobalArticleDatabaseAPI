using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Create new translation request
    /// </summary>
    public class UpdateTranslationRequest : BaseModel<UpdateTranslationRequest, UpdateTranslationRequestValidator>
    {
        /// <summary>
        /// Article unique id
        /// </summary>
        public string ArticleId { get; set; }

        /// <summary>
        /// Translation to be created
        /// </summary>
        public Translation Translation { get; set; }
    }

    public class UpdateTranslationRequestValidator : AbstractValidator<UpdateTranslationRequest>
    {
        public UpdateTranslationRequestValidator()
        {
            RuleFor(model => model.ArticleId).NotEmpty();
            RuleFor(model => model.Translation).NotEmpty();
            RuleFor(model => model.Translation.Date).NotEmpty();
            RuleFor(model => model.Translation.Language).NotEmpty();
            RuleFor(model => model.Translation.Status).NotEmpty();
        }
    }
}
