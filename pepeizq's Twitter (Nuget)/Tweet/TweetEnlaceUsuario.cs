using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetEnlaceUsuario
    {
        [JsonProperty("urls")]
        public TweetEnlace Enlaces { get; set; }
    }
}
