using Newtonsoft.Json;

namespace LambdaCore.Models
{
    public class WordpressAuthor
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

    }
}
