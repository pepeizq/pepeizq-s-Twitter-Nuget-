using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetEntidadUsuario
    {
        [JsonProperty("url")]
        public TweetEnlaceUsuario Enlace { get; set; }
    }
}

