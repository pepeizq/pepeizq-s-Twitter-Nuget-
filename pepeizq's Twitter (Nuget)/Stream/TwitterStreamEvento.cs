using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace pepeizq.Twitter.Stream
{
    public class TwitterStreamEvento : ITwitterResultado
    {
        [JsonProperty(PropertyName = "event")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TwitterStreamEventoTipo EventoTipo { get; set; }

        public TwitterUsuario Fuente { get; set; }

        public TwitterUsuario Objetivo { get; set; }

        public Tweet.Tweet ObjetivoTweet { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public string Creacion { get; set; }

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
