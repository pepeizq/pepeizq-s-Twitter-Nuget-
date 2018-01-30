using System;

namespace pepeizq.Twitter
{
    public class TwitterStatus
    {
        public string InReplyToStatusId { get; set; }

        public string Mensaje { get; set; }

        public string PlaceId { get; set; }

        public bool PossiblySensitive { get; set; }

        public string TweetID { get; set; }

        public string SolicitarParametrosRespuesta
        {
            get
            {
                string resultado = $"status={Uri.EscapeDataString(Mensaje)}";

                resultado = AddRequestParameter(resultado, "in_reply_to_status_id", InReplyToStatusId);
                resultado = AddRequestParameter(resultado, "place_id", PlaceId);
                resultado = AddRequestParameter(resultado, "possibly_sensitive", PossiblySensitive);
                resultado = AddRequestParameter(resultado, "trim_user", TrimUser);

                return resultado;
            }
        }

        public bool TrimUser { get; set; }

        private string AddRequestParameter(string request, string parameterName, bool valor)
        {
            var resultado = request;

            if (valor)
            {
                resultado = $"{resultado}&{parameterName}=true";
            }

            return resultado;
        }

        private string AddRequestParameter(string request, string parameterName, string valor)
        {
            var resultado = request;

            if (!string.IsNullOrEmpty(valor))
            {
                resultado = $"{resultado}&{parameterName}={Uri.EscapeDataString(valor)}";
            }

            return resultado;
        }
    }
}
