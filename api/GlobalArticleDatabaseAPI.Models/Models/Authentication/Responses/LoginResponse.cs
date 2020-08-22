namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Login response
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Authentication token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Renew token
        /// </summary>
        public string RenewToken { get; set; }
    }
}
