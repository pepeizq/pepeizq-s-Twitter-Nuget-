using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetMencion
    {
        [JsonProperty("screen_name")]
        public string ScreenNombre { get; set; }

        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("name")]
        public string Nombre { get; set; }

        [JsonProperty("indices")]
        public int[] Rango { get; set; }
    }
}
