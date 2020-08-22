using Newtonsoft.Json;
using System.IO;

namespace LambdaCore
{
    public class Config
    {
        private static Config _config;

        [JsonProperty("youtube_url")]
        public string YouTubeUrl { get; set; }

        [JsonProperty("solidjoys_playlist_id")]
        public string SolidJoysPlaylistId { get; set; }

        [JsonProperty("sdjc_channel_id")]
        public string SdjcChannelId { get; set; }

        [JsonProperty("youtube_api_key")]
        public string YoutubeApiKey { get; set; }

        [JsonProperty("wordpress_api_url")]
        public string WordpressApiUrl { get; set; }

        [JsonProperty("wordpress_jwt_url")]
        public string WordpressJwtUrl { get; set; }

        [JsonProperty("wordpress_solidjoys_author")]
        public int WordpressSolidJoysAuthor { get; set; }

        [JsonProperty("wordpress_solidjoys_feature_media_id")]
        public int WordpressSolidJoysFeatureMediaId { get; set; }

        [JsonProperty("wordpress_solidjoys_category_id")]
        public int WordpressSolidJoysCategoryId { get; set; }

        [JsonProperty("wordpress_article_category_id")]
        public int WordpressArticleCategoryId { get; set; }

        [JsonProperty("wordpress_solidjoys_template")]
        public string WordpressSolidJoysTemplate { get; set; }

        [JsonProperty("wordpress_article_template")]
        public string WordpressArticleTemplate { get; set; }

        [JsonProperty("wordpress_solidjoys_tag")]
        public int WordpressSolidJoysTag { get; set; }

        [JsonProperty("articles_schedule_url")]
        public string ArticlesScheduleUrl { get; set; }

        [JsonProperty("gadb_api")]
        public string GadbApi { get; set; }

        [JsonProperty("gadb_username")]
        public string GadbUsername { get; set; }

        [JsonProperty("gadb_password")]
        public string GadbPassword { get; set; }

        [JsonProperty("articles_topics_en_url")]
        public string ArticlesTopicsENUrl { get; set; }

        [JsonProperty("articles_topics_es_url")]
        public string ArticlesTopicsESUrl { get; set; }

        [JsonProperty("wordpress_username")]
        public string WordpressUsername { get; set; }

        [JsonProperty("wordpress_password")]
        public string WordpressPassword { get; set; }

        public static Config Settings
        {
            get
            {
                if (_config == null)
                {
                    _config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("appsettings.json"));
                }

                return _config;
            }
        }
    }
}
