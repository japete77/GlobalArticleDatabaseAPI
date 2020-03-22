using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Publication information
    /// </summary>
    public class Publication
    {
        /// <summary>
        /// Publish date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Organization where it was published
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// Hyperlink to the published article
        /// </summary>
        public string Link { get; set; }
    }
}
