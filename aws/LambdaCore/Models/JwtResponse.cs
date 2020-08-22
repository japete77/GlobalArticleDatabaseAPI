using Newtonsoft.Json;

namespace LambdaCore.Models
{
    public class JwtResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("user_email")]
        public string UserEmail { get; set; }

        [JsonProperty("user_nicename")]
        public string UserNiceName { get; set; }

        [JsonProperty("user_display_name")]
        public string UserDisplayName { get; set; }
    }
}
