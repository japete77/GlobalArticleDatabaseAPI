using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GlobalArticleDatabaseAPI.DbContext.Models
{
    public class AuthRenewEntity
    {
        /// <summary>
        /// Unique Id generated internally by the database
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        /// <summary>
        /// Renew token
        /// </summary>
        public string RenewToken { get; set; }
        /// <summary>
        /// Current user token. It must be a non expired one.
        /// </summary>
        public string UserToken { get; set; }
        /// <summary>
        /// Expiration date for the renewal token
        /// </summary>
        public DateTime ExpiteAt { get; set; }
    }
}
