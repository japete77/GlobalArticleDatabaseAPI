using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace GlobalArticleDatabaseAPI.Models
{
    public class Translation
    {
        /// <summary>
        /// Unique identifier of the translation
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// Unique identifier of the article
        /// </summary>
        public ObjectId ArticleId { get; set; }

        /// <summary>
        /// Translation date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Language of the translation
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Translation status (Outstanding, Assigned, In progress, Pending review, Reviewed, Completed)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Name of the translator
        /// </summary>
        public string TranslatedBy { get; set; }

        /// <summary>
        /// Name of the reviewer
        /// </summary>
        public string ReviewedBy { get; set; }

        /// <summary>
        /// Publications registry
        /// </summary>
        public List<Publication> Publications { get; set; }
    }
}
