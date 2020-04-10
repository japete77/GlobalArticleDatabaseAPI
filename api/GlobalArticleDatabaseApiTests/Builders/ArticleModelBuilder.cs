using GlobalArticleDatabaseAPI.Models;
using System;

namespace GlobalArticleDatabaseAPITests.Builders
{
    public class ArticleModelBuilder
    {
        private readonly Article _article;

        public ArticleModelBuilder()
        {
            _article = new Article();
        }

        public ArticleModelBuilder WithRandomValues()
        {
            _article.Author = $"author_{DateTime.Now.Ticks}";
            _article.Category = $"category_{DateTime.Now.Ticks}";
            _article.Date = DateTime.UtcNow;
            _article.Language = "en-US";
            _article.Owner = $"owner_{DateTime.Now.Ticks}";
            _article.SourceLink = $"https://source_{DateTime.Now.Ticks}";
            _article.Subtitle = $"Subtitles {DateTime.Now.Ticks}";
            _article.Summary = $"Summary {DateTime.Now.Ticks}";
            _article.Title = $"Title {DateTime.Now.Ticks}";
            return this;
        }

        public ArticleModelBuilder WithId(string id)
        {
            _article.Id = id;
            return this;
        }

        public ArticleModelBuilder WithAuthor(string author)
        {
            _article.Author = author;
            return this;
        }

        public ArticleModelBuilder WithDate(DateTime date)
        {
            _article.Date = date;
            return this;
        }

        public ArticleModelBuilder WithLanguage(string language)
        {
            _article.Language = language;
            return this;
        }

        public ArticleModelBuilder WithOwner(string owner)
        {
            _article.Owner = owner;
            return this;
        }

        public ArticleModelBuilder WithTitle(string title)
        {
            _article.Title = title;
            return this;
        }


        public Article Build() => _article;
    }
}
