using Newtonsoft.Json;

namespace LambdaCore.Models
{
    public class WordpressMedia
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
