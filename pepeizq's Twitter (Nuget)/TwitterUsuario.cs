using Microsoft.Toolkit.Services;
using Newtonsoft.Json;
using pepeizq.Twitter.OAuth;

namespace pepeizq.Twitter
{
    public class TwitterUsuario : SchemaBase, ITwitterResultado
    {
        [JsonProperty("id_str")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Nombre { get; set; }

        [JsonProperty("screen_name")]
        public string ScreenNombre { get; set; }

        [JsonProperty("profile_image_url_https")]
        public string ImagenAvatar { get; set; }

        [JsonProperty("verified")]
        public bool Verificado { get; set; }

        [JsonProperty("following")]
        public string Siguiendo { get; set; }

        [JsonProperty("created_at")]
        public string Creacion { get; set; }

        [JsonProperty("profile_sidebar_fill_color")]
        public string ColorRelleno { get; set; }

        [JsonProperty("profile_sidebar_border_color")]
        public string ColorBorde { get; set; }

        [JsonProperty("profile_text_color")]
        public string ColorTexto { get; set; }

        [JsonProperty("profile_link_color")]
        public string ColorEnlace { get; set; }

        [JsonProperty("profile_background_color")]
        public string ColorFondo { get; set; }

        [JsonProperty("lang")]
        public string Idioma { get; set; }

        [JsonProperty("statuses_count")]
        public string NumTweets { get; set; }

        [JsonProperty("followers_count")]
        public string Followers { get; set; }

        [JsonProperty("favourites_count")]
        public string Favoritos { get; set; }

        [JsonProperty("friends_count")]
        public string Amigos { get; set; }

        [JsonProperty("listed_count")]
        public string Listas { get; set; }

        [JsonProperty("description")]
        public string Descripcion { get; set; }

        [JsonProperty("url")]
        public string Enlace { get; set; }

        public TwitterOAuthTokens Tokens { get; set; }

        [JsonProperty("entities")]
        public Tweet.TweetEntidadUsuario Entidades { get; set; }
    }
}
