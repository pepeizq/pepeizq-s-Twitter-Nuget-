using Microsoft.Toolkit.Parsers;
using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class Tweet : SchemaBase, ITwitterResultado
    {
        [JsonProperty("created_at")]
        public string Creacion { get; set; }

        [JsonProperty("id_str")]
        public string ID { get; set; }

        [JsonProperty("full_text")]
        public string TextoCompleto { get; set; }

        [JsonProperty("text")]
        public string TextoParcial { get; set; }

        [JsonProperty("user")]
        public TwitterUsuario Usuario { get; set; }

        [JsonProperty("entities")]
        public TweetEntidad Entidades { get; set; }

        [JsonProperty("extended_entities")]
        public TweetEntidadExtendida EntidadesExtendida { get; set; }

        [JsonProperty("retweeted_status")]
        public Tweet Retweet { get; set; }

        [JsonProperty("quoted_status")]
        public Tweet Cita { get; set; }

        [JsonProperty("in_reply_to_screen_name")]
        public string RespuestaUsuarioScreenNombre { get; set; }

        [JsonProperty("in_reply_to_status_id")]
        public string RespuestaUsuarioID { get; set; }

        [JsonProperty("display_text_range")]
        public int[] TextoRango { get; set; }

        [JsonProperty("retweeted")]
        public bool Retwitteado { get; set; }

        [JsonProperty("retweet_count")]
        public int NumRetweets { get; set; }

        [JsonProperty("favorited")]
        public bool Favoriteado { get; set; }

        [JsonProperty("favorite_count")]
        public int NumFavoritos { get; set; }

        [JsonProperty("reply_count")]
        public int NumRespuestas { get; set; }

        [JsonProperty("lang")]
        public string Idioma { get; set; }

        [JsonProperty("source")]
        public string ClienteUsado { get; set; }
    }
}