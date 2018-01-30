using Newtonsoft.Json;

namespace pepeizq.Twitter
{
    public class TwitterErrores
    {
        [JsonProperty("errors")]
        public TwitterError[] Errores { get; set; }
    }
}
