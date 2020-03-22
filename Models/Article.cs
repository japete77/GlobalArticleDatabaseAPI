using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Model for an article
    /// </summary>
    public class Article
    {
        /// <summary>
        /// Unique identifier of the article
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// Article creation date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Author of the article
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Category of the article (Article, Interview, Sermon, etc)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Title of the article
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Subtitle of the article
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// Brief summary of the article content
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Original language of the article
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Original source hyperlink of the article
        /// </summary>
        public string SourceLink { get; set; }

        /// <summary>
        /// Organization that owns the rights of the article
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Indicate whether the article has image
        /// </summary>
        public bool HasImage { get; set; }

        /// <summary>
        /// Link to the source text
        /// </summary>
        [BsonIgnore]
        public string TextLink { get; set; }

        /// <summary>
        /// Link to the article image
        /// </summary>
        [BsonIgnore]
        public string ImageLink { get; set; }
    }
}
