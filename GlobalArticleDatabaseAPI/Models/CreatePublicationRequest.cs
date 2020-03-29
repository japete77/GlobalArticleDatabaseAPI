using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Create new translation request
    /// </summary>
    public class CreatePublicationRequest : BaseModel<CreatePublicationRequest, CreatePublicationRequestValidator>
    {
        /// <summary>
        /// Article unique id
        /// </summary>
        public string ArticleId { get; set; }

        /// <summary>
        /// Translation language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Publication to be created
        /// </summary>
        public Publication Publication { get; set; }
    }

    public class CreatePublicationRequestValidator : AbstractValidator<CreatePublicationRequest>
    {
        public CreatePublicationRequestValidator()
        {
            RuleFor(model => model.ArticleId).NotEmpty();
            RuleFor(model => model.Language).NotEmpty();
            RuleFor(model => model.Publication).NotEmpty();
            RuleFor(model => model.Publication.Publisher).NotEmpty();
        }
    }
}
