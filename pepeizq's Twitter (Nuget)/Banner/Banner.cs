using Microsoft.Toolkit.Parsers;
using Newtonsoft.Json;

namespace pepeizq.Twitter.Banner
{
    public class Banner : SchemaBase, ITwitterResultado
    {
        [JsonProperty("sizes")]
        public BannerTamaño Tamaños { get; set; }
    }
}
