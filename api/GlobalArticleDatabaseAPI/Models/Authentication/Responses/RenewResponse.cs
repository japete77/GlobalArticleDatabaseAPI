namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Renew response
    /// </summary>
    public class RenewResponse
    {
        /// <summary>
        /// Authentication token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// Renewal token
        /// </summary>
        public string RenewalToken { get; set; }
    }
}
