using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace pepeizq.Twitter.OAuth
{
    internal class TwitterOAuthRequestConstructor
    {
        public const string Realm = "Twitter API";

        public string Verb { get; set; }

        public Uri EncodedRequestUri { get; private set; }

        public Uri RequestUriWithoutQuery { get; private set; }

        public IEnumerable<OAuthParametro> QueryParams { get; private set; }

        public OAuthParametro Version { get; private set; }

        public OAuthParametro Nonce { get; private set; }

        public OAuthParametro Timestamp { get; private set; }

        public OAuthParametro SignatureMethod { get; private set; }

        public OAuthParametro ConsumerKey { get; private set; }

        public OAuthParametro ConsumerSecret { get; private set; }

        public OAuthParametro Token { get; private set; }

        public OAuthParametro TokenSecret { get; private set; }

        public OAuthParametro Signature => new OAuthParametro("oauth_signature", GenerarFirma());

        public string AuthorizationHeader => GenerarCabeceraAutorizada();

        public TwitterOAuthRequestConstructor(Uri requestUri, TwitterOAuthTokens tokens, string method = "GET")
        {
            Verb = method;

            RequestUriWithoutQuery = new Uri(requestUri.AbsolutaSinQuery());

            if (!string.IsNullOrEmpty(requestUri.Query))
            {
                QueryParams = requestUri.CogerQueryParametros()
                    .Select(p => new OAuthParametro(p.Key, Uri.UnescapeDataString(p.Value)))
                    .ToList();
            }
            else
            {
                QueryParams = new List<OAuthParametro>();
            }

            EncodedRequestUri = GetEncodedUri(requestUri, QueryParams);

            Version = new OAuthParametro("oauth_version", "1.0");
            Nonce = new OAuthParametro("oauth_nonce", GenerarNonce());
            Timestamp = new OAuthParametro("oauth_timestamp", GenerarTiempoLapso());
            SignatureMethod = new OAuthParametro("oauth_signature_method", "HMAC-SHA1");
            ConsumerKey = new OAuthParametro("oauth_consumer_key", tokens.ConsumerKey);
            ConsumerSecret = new OAuthParametro("oauth_consumer_secret", tokens.ConsumerSecret);
            Token = new OAuthParametro("oauth_token", tokens.AccessToken);
            TokenSecret = new OAuthParametro("oauth_token_secret", tokens.AccessTokenSecret);
        }

        private static Uri GetEncodedUri(Uri requestUri, IEnumerable<OAuthParametro> parametros)
        {
            StringBuilder requestParametersBuilder = new StringBuilder(requestUri.AbsolutaSinQuery());
            var oAuthParametros = parametros as OAuthParametro[] ?? parametros.ToArray();
            if (oAuthParametros.Any())
            {
                requestParametersBuilder.Append("?");

                foreach (var queryParam in oAuthParametros)
                {
                    requestParametersBuilder.AppendFormat("{0}&", queryParam.ToString());
                }

                requestParametersBuilder.Remove(requestParametersBuilder.Length - 1, 1);
            }

            return new Uri(requestParametersBuilder.ToString());
        }

        private static string GenerarNonce()
        {
            return new Random().Next(123400, int.MaxValue).ToString("X", CultureInfo.InvariantCulture);
        }

        private static string GenerarTiempoLapso()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
        }

        private string GenerarFirma()
        {
            string signatureBaseString = string.Format(
                CultureInfo.InvariantCulture,
                "{2}&{0}&{1}",
                OAuthEncoder.UrlEncode(RequestUriWithoutQuery.Normalizar()),
                OAuthEncoder.UrlEncode(GetSignParameters()),
                Verb);

            string key = string.Format(
                CultureInfo.InvariantCulture,
                "{0}&{1}",
                OAuthEncoder.UrlEncode(ConsumerSecret.Valor),
                OAuthEncoder.UrlEncode(TokenSecret.Valor));

            return OAuthEncoder.GenerarHash(signatureBaseString, key);
        }

        private string GenerarCabeceraAutorizada()
        {
            StringBuilder authHeaderBuilder = new StringBuilder();

            authHeaderBuilder.AppendFormat("OAuth realm=\"{0}\",", Realm);
            authHeaderBuilder.Append(string.Join(",", GetAuthHeaderParameters().OrderBy(p => p.Clave).Select(p => p.ToString(true)).ToArray()));
            authHeaderBuilder.AppendFormat(",{0}", Signature.ToString(true));

            return authHeaderBuilder.ToString();
        }

        private IEnumerable<OAuthParametro> GetSignParameters()
        {
            foreach (var queryParam in QueryParams)
            {
                yield return queryParam;
            }

            yield return Version;
            yield return Nonce;
            yield return Timestamp;
            yield return SignatureMethod;
            yield return ConsumerKey;
            yield return Token;
        }

        private IEnumerable<OAuthParametro> GetAuthHeaderParameters()
        {
            yield return Version;
            yield return Nonce;
            yield return Timestamp;
            yield return SignatureMethod;
            yield return ConsumerKey;
            yield return Token;
        }
    }
}