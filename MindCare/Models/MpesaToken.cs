using Newtonsoft.Json;

namespace MindCare.Models
{
    public class MPesaToken
    {
        [JsonProperty("access_token")]
        public string access_token { get; set; }

        [JsonProperty("expires_in")]
        public string ExpiresIn { get; set; }
    }
}
