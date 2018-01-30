using Newtonsoft.Json;

namespace pepeizq.Twitter
{
    public class TwitterUsuario
    {
        [JsonProperty("id_str")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Nombre { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenNombre { get; set; }

        [JsonProperty("profile_image_url")]
        public string Avatar { get; set; }
    }
}
