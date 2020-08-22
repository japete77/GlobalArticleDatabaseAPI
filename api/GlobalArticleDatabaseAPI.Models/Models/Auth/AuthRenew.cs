using System;

namespace GlobalArticleDatabaseAPI.Models
{
    public class AuthRenew
    {
        /// <summary>
        /// Unique Id generated internally by the database
        /// </summary>
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
