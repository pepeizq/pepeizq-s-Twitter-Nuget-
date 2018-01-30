using Newtonsoft.Json;

namespace pepeizq.Twitter
{
    public class TwitterError
    {
        [JsonProperty("code")]
        public int Codigo { get; set; }

        [JsonProperty("message")]
        public string Mensaje { get; set; }
    }
}
