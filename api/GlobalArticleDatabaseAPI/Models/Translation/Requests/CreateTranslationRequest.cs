using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Create new translation request
    /// </summary>
    public class CreateTranslationRequest : BaseModel<CreateTranslationRequest, CreateTranslationRequestValidator>
    {
        /// <summary>
        /// Article unique id
        /// </summary>
        public string ArticleId { get; set; }

        /// <summary>
        /// Translation content
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Translation to be created
        /// </summary>
        public Translation Translation { get; set; }
    }

    public class CreateTranslationRequestValidator : AbstractValidator<CreateTranslationRequest>
    {
        public CreateTranslationRequestValidator()
        {
            RuleFor(model => model.ArticleId).NotEmpty();
            RuleFor(model => model.Translation).NotEmpty();
            RuleFor(model => model.Translation.Date).NotEmpty();
            RuleFor(model => model.Translation.Language).NotEmpty();
        }
    }
}
