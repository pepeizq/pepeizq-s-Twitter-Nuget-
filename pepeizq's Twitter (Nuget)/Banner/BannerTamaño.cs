using Newtonsoft.Json;

namespace pepeizq.Twitter.Banner
{
   public class BannerTamaño
    {
        [JsonProperty("600x200")]
        public BannerImagen I600x200 { get; set; }

        [JsonProperty("1500x500")]
        public BannerImagen I1500x500 { get; set; }
    }
}
