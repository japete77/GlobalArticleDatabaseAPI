using System.Collections.Generic;

namespace GlobalArticleDatabaseAPI.Models
{
    public static class TranslationStatus
    {
        public static string PENDING_REVIEW = "Pending Review";
        public static string REVIEW_IN_PROGRESS = "Review In Progress";
        public static string REVIEW_COMPLETED = "Review Completed";
        public static string PUBLISHED = "Published";

        public static List<string> GetTraslationStatus()
        {
            return new List<string>
            {
                PENDING_REVIEW,
                REVIEW_IN_PROGRESS,
                REVIEW_COMPLETED,
                PUBLISHED
            };
        }
    }
}
