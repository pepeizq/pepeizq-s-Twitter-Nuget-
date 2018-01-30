using Newtonsoft.Json;
namespace pepeizq.Twitter.Stream
{
    public class TwitterStreamEventoBorrar : ITwitterResultado
    {
        [JsonProperty(PropertyName = "user_id_str")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "id_str")]
        public string Id { get; set; }
    }
}
