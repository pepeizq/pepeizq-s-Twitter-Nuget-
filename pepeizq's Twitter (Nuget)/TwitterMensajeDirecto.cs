using Newtonsoft.Json;
using pepeizq.Twitter.Tweet;
using System;
using System.Globalization;

namespace pepeizq.Twitter
{
    public class TwitterMensajeDirecto : ITwitterResultado
    {
        [JsonProperty("id")]
        public decimal IDMensaje { get; set; }

        [JsonProperty("sender_id")]
        public decimal IDEnviador { get; set; }

        [JsonProperty("text")]
        public string Texto { get; set; }

        [JsonProperty("recipient_id")]
        public decimal IDDestinatario { get; set; }

        [JsonProperty("created_at")]
        public string Creacion { get; set; }

        [JsonProperty("sender_screen_name")]
        public string ScreenNombreEnviador { get; set; }

        [JsonProperty("recipient_screen_name")]
        public string ScreenNombreDestinatario { get; set; }

        [JsonProperty("sender")]
        public TwitterUsuario Enviador { get; set; }

        [JsonProperty("recipient")]
        public TwitterUsuario Destinatario { get; set; }

        [JsonProperty("entities")]
        public TweetEntidad Entidades { get; set; }

        public DateTime CreacionFecha
        {
            get
            {
                DateTime dt;
                if (!DateTime.TryParseExact(Creacion, "ddd MMM dd HH:mm:ss zzzz yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    dt = DateTime.Today;
                }

                return dt;
            }
        }
    }
}
