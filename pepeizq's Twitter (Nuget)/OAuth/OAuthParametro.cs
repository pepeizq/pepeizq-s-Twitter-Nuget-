using System.Globalization;

namespace pepeizq.Twitter.OAuth
{
    internal class OAuthParametro
    {
        public string Clave { get; set; }

        public string Valor { get; set; }

        public OAuthParametro(string clave, string valor)
        {
            Clave = clave;
            Valor = valor;
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool corchetes)
        {
            string format = null;

            if (corchetes)
            {
                format = "{0}=\"{1}\"";
            }
            else
            {
                format = "{0}={1}";
            }

            return string.Format(CultureInfo.InvariantCulture, format, OAuthEncoder.UrlEncode(Clave), OAuthEncoder.UrlEncode(Valor));
        }
    }
}
