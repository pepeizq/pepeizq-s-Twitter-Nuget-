using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetEnlace
    {
        [JsonProperty("url")]
        public string Enlace { get; set; }

        [JsonProperty("expanded_url")]
        public string Expandida { get; set; }

        [JsonProperty("display_url")]
        public string Mostrar { get; set; }

        [JsonProperty("indices")]
        public int[] Rango { get; set; }
    }
}
