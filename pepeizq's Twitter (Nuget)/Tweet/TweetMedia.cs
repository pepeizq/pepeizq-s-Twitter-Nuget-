using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetMedia
    {
        [JsonProperty("media_url")]
        public string Enlace { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("media_url_https")]
        public string EnlaceHttps { get; set; }

        [JsonProperty("type")]
        public string Tipo { get; set; }

        [JsonProperty("video_info")]
        public TweetVideo Video { get; set; }
    }
}
