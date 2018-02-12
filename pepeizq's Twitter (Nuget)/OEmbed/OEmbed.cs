using Microsoft.Toolkit.Services;
using Newtonsoft.Json;

namespace pepeizq.Twitter.OEmbed
{
    public class OEmbed : SchemaBase, ITwitterResultado
    {
        [JsonProperty("url")]
        public string Enlace { get; set; }

        [JsonProperty("html")]
        public string Html { get; set; }

        [JsonProperty("width")]
        public string Ancho { get; set; }

        [JsonProperty("height")]
        public string Alto { get; set; }
    }
}
