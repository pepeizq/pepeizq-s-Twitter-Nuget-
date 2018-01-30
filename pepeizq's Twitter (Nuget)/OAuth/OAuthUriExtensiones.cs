using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Windows.Foundation;

namespace pepeizq.Twitter.OAuth
{
    internal static class OAuthUriExtensiones
    {
        public static IDictionary<string, string> CogerQueryParametros(this Uri uri)
        {
            return new WwwFormUrlDecoder(uri.Query).ToDictionary(decoderEntrada => decoderEntrada.Name, decoderEntrada => decoderEntrada.Value);
        }

        public static string AbsolutaSinQuery(this Uri uri)
        {
            if (string.IsNullOrEmpty(uri.Query))
            {
                return uri.AbsoluteUri;
            }

            return uri.AbsoluteUri.Replace(uri.Query, string.Empty);
        }

        public static string Normalizar(this Uri uri)
        {
            var resultado = new StringBuilder(string.Format(CultureInfo.InvariantCulture, "{0}://{1}", uri.Scheme, uri.Host));

            if (!((uri.Scheme == "http" && uri.Port == 80) || (uri.Scheme == "https" && uri.Port == 443)))
            {
                resultado.Append(string.Concat(":", uri.Port));
            }

            resultado.Append(uri.AbsolutePath);

            return resultado.ToString();
        }
    }
}