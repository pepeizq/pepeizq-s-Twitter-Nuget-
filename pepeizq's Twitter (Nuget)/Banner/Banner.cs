using Microsoft.Toolkit.Services;
using Newtonsoft.Json;

namespace pepeizq.Twitter.Banner
{
    public class Banner : SchemaBase, ITwitterResultado
    {
        [JsonProperty("sizes")]
        public BannerTamaño Tamaños { get; set; }
    }
}
