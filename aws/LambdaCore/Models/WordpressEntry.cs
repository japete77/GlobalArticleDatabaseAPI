using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace LambdaCore.Models
{
    public class WordpressEntryBase
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("author")]
        public int Author { get; set; }

        [JsonProperty("featured_media")]
        public int FeaturedMedia { get; set; }

        [JsonProperty("format")]
        public string Format { get; set; }

        [JsonProperty("categories")]
        public List<int> Categories { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("template")]
        public string Template { get; set; }

        [JsonProperty("tags")]
        public List<int> Tags { get; set; }
    }

    public class WordpressEntry : WordpressEntryBase
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class WordpressEntryResult : WordpressEntryBase
    {
        [JsonProperty("title")]
        public WordpressRendered Title { get; set; }

        [JsonProperty("content")]
        public WordpressRendered Content { get; set; }
    }

    public class WordpressRendered
    {
        [JsonProperty("rendered")]
        public string Rendered { get; set; }
    }
}
