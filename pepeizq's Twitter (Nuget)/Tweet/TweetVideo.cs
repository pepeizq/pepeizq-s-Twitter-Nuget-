using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetVideo
    {
        [JsonProperty("aspect_ratio")]
        public int[] Ratio { get; set; }

        [JsonProperty("duration_millis")]
        public string DuracionMilisegundos { get; set; }

        [JsonProperty("variants")]
        public TweetVideoVariante[] Variantes { get; set; }
    }
}
