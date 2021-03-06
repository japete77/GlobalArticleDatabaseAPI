﻿using System;
using System.Collections.Generic;

namespace GlobalArticleDatabaseAPI.Models
{
    public class Translation
    {
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
        /// Indicates of the translation has text
        /// </summary>
        public bool HasText { get; set; }

        /// <summary>
        /// Link to the translation text
        /// </summary>
        public string TextLink { get; set; }

        /// <summary>
        /// Publications registry
        /// </summary>
        public List<Publication> Publications { get; set; }
    }
}
