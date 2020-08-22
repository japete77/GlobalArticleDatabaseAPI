using Newtonsoft.Json;

namespace LambdaCore.Models
{
    public class WordpressTopic
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

    }
}
