using System.Collections.Generic;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Search user response
    /// </summary>
    public class SearchUsersResponse
    {
        /// <summary>
        /// Current page number
        /// </summary>
        public long CurrentPage { get; set; }
        /// <summary>
        /// Total number of users
        /// </summary>
        public long Total { get; set; }
        /// <summary>
        /// User details list
        /// </summary>
        public List<User> Users { get; set; }
        /// <summary>
        /// Next page url
        /// </summary>
        public string Next { get; set; }
    }
}
