using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetVideoVariante
    {
        [JsonProperty("bitrate")]
        public int Bitrate { get; set; }

        [JsonProperty("content_type")]
        public string Tipo { get; set; }

        [JsonProperty("url")]
        public string Enlace { get; set; }
    }
}
