using Newtonsoft.Json;

namespace pepeizq.Twitter.Banner
{
    public class BannerImagen
    {
        [JsonProperty("h")]
        public string Altura { get; set; }

        [JsonProperty("w")]
        public string Anchura { get; set; }

        [JsonProperty("url")]
        public string Enlace { get; set; }
    }
}
