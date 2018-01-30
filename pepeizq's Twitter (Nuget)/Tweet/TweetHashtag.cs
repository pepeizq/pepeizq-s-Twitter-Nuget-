using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetHashtag
    {
        [JsonProperty("text")]
        public string Nombre { get; set; }

        [JsonProperty("indices")]
        public int[] Rango { get; set; }
    }
}
