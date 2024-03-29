﻿using System;
using System.Collections.Generic;

namespace GlobalArticleDatabaseAPI.Models
{
    /// <summary>
    /// Model for an article
    /// </summary>
    public class Article
    {
        /// <summary>
        /// Unique identifier of the article
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Article creation date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Authors of the article
        /// </summary>
        public List<string> Author { get; set; }

        /// <summary>
        /// Category of the article (Article, Interview, Sermon, etc)
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Topics of the article
        /// </summary>
        public List<string> Topics { get; set; }

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
        /// Original language of the article
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Original source hyperlink of the article
        /// </summary>
        public string SourceLink { get; set; }

        /// <summary>
        /// Organization that owns the rights of the article
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Bible reference
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Number of words in the article, including Title and Summary
        /// </summary>
        public int Words { get; set; }

        /// <summary>
        /// Number of characters in the article including Title and Summary
        /// </summary>
        public int Characters { get; set; }

        /// <summary>
        /// Indicate whether the article has image
        /// </summary>
        public bool HasText { get; set; }

        /// <summary>
        /// Link to the source text
        /// </summary>
        public string TextLink { get; set; }

        /// <summary>
        /// Link to the article image
        /// </summary>
        public string ImageLink { get; set; }

        /// <summary>
        /// Article translations
        /// </summary>
        public List<Translation> Translations { get; set; }
    }
}
