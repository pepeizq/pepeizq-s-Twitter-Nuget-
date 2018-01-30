using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetEntidad
    {
        [JsonProperty("media")]
        public TweetMedia[] Media { get; set; }

        [JsonProperty("urls")]
        public TweetEnlace[] Enlaces { get; set; }

        [JsonProperty("user_mentions")]
        public TweetMencion[] Menciones { get; set; }

        [JsonProperty("hashtags")]
        public TweetHashtag[] Hashtags { get; set; }
    }
}
