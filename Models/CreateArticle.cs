namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Article entry
    /// </summary>
    public class CreateArticle
    {
        /// <summary>
        /// Article metadata
        /// </summary>
        public Article Article { get; set; }

        /// <summary>
        /// Article text content
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Article jpg image encoded in Base64 
        /// </summary>
        public string ImageBase64 { get; set; }
    }
}
