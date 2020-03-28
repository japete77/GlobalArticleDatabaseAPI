using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Delete translation request
    /// </summary>
    public class DeleteTranslationRequest : BaseModel<DeleteTranslationRequest, DeleteTranslationRequestValidator>
    {
        /// <summary>
        /// Article unique id
        /// </summary>
        public string ArticleId { get; set; }

        /// <summary>
        /// Translation language
        /// </summary>
        public string Language { get; set; }
    }

    public class DeleteTranslationRequestValidator : AbstractValidator<DeleteTranslationRequest>
    {
        public DeleteTranslationRequestValidator()
        {
            RuleFor(model => model.ArticleId).NotEmpty();
            RuleFor(model => model.Language).NotEmpty();
        }
    }
}
