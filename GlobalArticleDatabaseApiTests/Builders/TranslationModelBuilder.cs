using GlobalArticleDatabaseAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GlobalArticleDatabaseApiTests.Builders
{
    public class TranslationModelBuilder
    {
        private readonly Translation _translation;

        public TranslationModelBuilder()
        {
            _translation = new Translation();
        }

        public TranslationModelBuilder WithRandomValues()
        {
            _translation.Title = $"Title_{DateTime.Now.Ticks}";
            _translation.Subtitle = $"Subtitle_{DateTime.Now.Ticks}";
            _translation.Summary = $"Summary_{DateTime.Now.Ticks}";
            _translation.Date = DateTime.UtcNow;
            _translation.Language = "en";
            _translation.ReviewedBy = $"Reviewer_{DateTime.Now.Ticks}";
            _translation.Status = $"Status_{DateTime.Now.Ticks}";
            _translation.TranslatedBy = $"Translator_{DateTime.Now.Ticks}";

            return this;
        }

        public TranslationModelBuilder WithLanguage(string language)
        {
            _translation.Language = language;
            return this;
        }

        public TranslationModelBuilder WithTitle(string title)
        {
            _translation.Title = title;
            return this;
        }

        public TranslationModelBuilder WithStatus(string status)
        {
            _translation.Status = status;
            return this;
        }

        public TranslationModelBuilder WithDate(DateTime date)
        {
            _translation.Date = date;
            return this;
        }

        public Translation Build() => _translation;
    }
}
