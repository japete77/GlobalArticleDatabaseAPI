using FluentValidation;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Article text update request
    /// </summary>
    public class UpdateArticleImageRequest : BaseModel<UpdateArticleImageRequest, UpdateArticleImageRequestValidator>
    {
        /// <summary>
        /// Article unique identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Article image content
        /// </summary>
        public string ImageBase64 { get; set; }
    }


    public class UpdateArticleImageRequestValidator : AbstractValidator<UpdateArticleImageRequest>
    {
        public UpdateArticleImageRequestValidator()
        {
            RuleFor(model => model.Id).NotEmpty();
            RuleFor(model => model.ImageBase64).NotEmpty();
        }
    }
}
