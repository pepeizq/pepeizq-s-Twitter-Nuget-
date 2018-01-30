using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetEntidadExtendida
    {
        [JsonProperty("media")]
        public TweetMedia[] Media { get; set; }
    }
}
