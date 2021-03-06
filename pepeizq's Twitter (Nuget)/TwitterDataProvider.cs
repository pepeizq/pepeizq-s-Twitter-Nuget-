﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Toolkit.Parsers;
using Microsoft.Toolkit.Services;
using pepeizq.Twitter.OAuth;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace pepeizq.Twitter
{
    public class TwitterDataProvider : DataProviderBase<TwitterDataConfig, SchemaBase>
    {
        private const string BaseUrl = "https://api.twitter.com/1.1";
        private const string OAuthBaseUrl = "https://api.twitter.com/oauth";
        private const string PublishUrl = "https://upload.twitter.com/1.1";
        private const string UserStreamUrl = "https://userstream.twitter.com/1.1";

        private static HttpClient _client;

        public readonly TwitterOAuthTokens _tokens;

        private readonly PasswordVault boveda;

        public string UsuarioScreenNombre { get; set; }

        public bool Logeado { get; private set; }

        public TwitterDataProvider(TwitterOAuthTokens tokens)
        {
            _tokens = tokens;
            boveda = new PasswordVault();

            if (_client == null)
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip
                };

                _client = new HttpClient(handler);
            }
        }

        public async Task<IEnumerable<TSchema>> SearchAsync<TSchema>(string hashTag, int maxRecords, IParser<TSchema> parser)
            where TSchema : SchemaBase
        {
            try
            {
                var uri = new Uri($"{BaseUrl}/search/tweets.json?q={Uri.EscapeDataString(hashTag)}&count={maxRecords}");
                TwitterOAuthRequest request = new TwitterOAuthRequest();
                var rawResult = await request.EjecutarGetAsync(uri, _tokens);

                var result = parser.Parse(rawResult);
                return result
                        .Take(maxRecords)
                        .ToList();
            }
            catch (WebException wex)
            {
                if (wex.Response is HttpWebResponse response)
                {
                    if ((int)response.StatusCode == 429)
                    {
                        throw new TooManyRequestsException();
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new OAuthKeysRevokedException();
                    }
                }

                throw;
            }
        }

        private PasswordCredential PasswordCredencial
        {
            get
            {
                var passwordCredentials = boveda.RetrieveAll();
                string usuario = UsuarioScreenNombre;

                var temp = passwordCredentials.FirstOrDefault(c => c.Resource == usuario);

                if (temp == null)
                {
                    return null;
                }

                return boveda.Retrieve(temp.Resource, temp.UserName);
            }
        }

        public async Task<bool> Logear()
        {
            UsuarioScreenNombre = (string)ApplicationData.Current.LocalSettings.Values["TwitterScreenNombre"];

            var twitterCredenciales = PasswordCredencial;
            if (twitterCredenciales != null)
            {
                _tokens.AccessToken = twitterCredenciales.UserName;
                _tokens.AccessTokenSecret = twitterCredenciales.Password;
                UsuarioScreenNombre = twitterCredenciales.Resource;
                Logeado = true;
                return true;
            }

            if (await InitializeRequestAccessTokensAsync(_tokens.CallbackEnlace) == false)
            {
                Logeado = false;
                return false;
            }

            string requestToken = _tokens.RequestToken;
            string twitterEnlace = $"{OAuthBaseUrl}/authorize?oauth_token={requestToken}";

            Uri startUri = new Uri(twitterEnlace);
            Uri endUri = new Uri(_tokens.CallbackEnlace);

            var resultado = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);

            switch (resultado.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:
                    Logeado = true;
                    return await ExchangeRequestTokenForAccessTokenAsync(resultado.ResponseData);
                case WebAuthenticationStatus.ErrorHttp:
                    Debug.WriteLine("WAB failed, message={0}", resultado.ResponseErrorDetail.ToString());
                    Logeado = false;
                    return false;
                case WebAuthenticationStatus.UserCancel:
                    Debug.WriteLine("WAB user aborted.");
                    Logeado = false;
                    return false;
            }

            Logeado = false;
            return false;
        }

        public void Deslogear()
        {
            var twitterCredenciales = PasswordCredencial;
            if (twitterCredenciales != null)
            {
                boveda.Remove(twitterCredenciales);
                UsuarioScreenNombre = null;
            }

            Logeado = false;
        }

        //--------------------------------------------

        protected override IParser<SchemaBase> GetDefaultParser(TwitterDataConfig config)
        {
            if (config == null)
            {
                throw new ConfigNullException();
            }

            switch (config.QueryTipo)
            {
                case TwitterQueryTipo.Busqueda:
                case TwitterQueryTipo.Inicio:
                case TwitterQueryTipo.Usuario:
                case TwitterQueryTipo.Banner:
                case TwitterQueryTipo.Personalizada:
                    return new TwitterParser<SchemaBase>();
                default:
                    return new TwitterParser<SchemaBase>();
            }
        }

        protected override async Task<IEnumerable<TSchema>> GetDataAsync<TSchema>(TwitterDataConfig config, int maxRecords, int pageIndex, IParser<TSchema> parser)
        {
            IEnumerable<TSchema> items;
            switch (config.QueryTipo)
            {
                case TwitterQueryTipo.Usuario:
                case TwitterQueryTipo.Busqueda:
                    items = await SearchAsync(config.Query, maxRecords, parser);
                    break;
                case TwitterQueryTipo.Inicio:
                case TwitterQueryTipo.Banner:
                case TwitterQueryTipo.Personalizada:
                    items = await GetCustomSearch(config.Query, parser);
                    break;
                default:
                    items = null;
                    break;
            }

            return items;
        }

        protected override void ValidateConfig(TwitterDataConfig config)
        {
            if (config?.Query == null && config?.QueryTipo != TwitterQueryTipo.Inicio)
            {
                throw new ConfigParameterNullException(nameof(config.Query));
            }

            if (_tokens == null)
            {
                throw new ConfigParameterNullException(nameof(_tokens));
            }

            if (string.IsNullOrEmpty(_tokens.ConsumerKey))
            {
                throw new OAuthKeysNotPresentException(nameof(_tokens.ConsumerKey));
            }

            if (string.IsNullOrEmpty(_tokens.ConsumerSecret))
            {
                throw new OAuthKeysNotPresentException(nameof(_tokens.ConsumerSecret));
            }
        }

        private static string ExtractTokenFromResponse(string getResponse, TwitterOAuthTokenTipo tokenType)
        {
            string requestOrAccessToken = null;
            string requestOrAccessTokenSecret = null;
            string oauthVerifier = null;
            string oauthCallbackConfirmed = null;
            string screenName = null;
            string[] keyValPairs = getResponse.Split('&');

            for (int i = 0; i < keyValPairs.Length; i++)
            {
                string[] splits = keyValPairs[i].Split('=');
                switch (splits[0])
                {
                    case "screen_name":
                        screenName = splits[1];
                        break;
                    case "oauth_token":
                        requestOrAccessToken = splits[1];
                        break;
                    case "oauth_token_secret":
                        requestOrAccessTokenSecret = splits[1];
                        break;
                    case "oauth_callback_confirmed":
                        oauthCallbackConfirmed = splits[1];
                        break;
                    case "oauth_verifier":
                        oauthVerifier = splits[1];
                        break;
                }
            }

            switch (tokenType)
            {
                case TwitterOAuthTokenTipo.OAuthRequestOrAccessToken:
                    return requestOrAccessToken;
                case TwitterOAuthTokenTipo.OAuthRequestOrAccessTokenSecret:
                    return requestOrAccessTokenSecret;
                case TwitterOAuthTokenTipo.OAuthVerifier:
                    return oauthVerifier;
                case TwitterOAuthTokenTipo.ScreenName:
                    return screenName;
                case TwitterOAuthTokenTipo.OAuthCallbackConfirmed:
                    return oauthCallbackConfirmed;
            }

            return string.Empty;
        }

       

        private async Task<IEnumerable<TSchema>> GetCustomSearch<TSchema>(string query, IParser<TSchema> parser)
            where TSchema : SchemaBase
        {
            try
            {
                var uri = new Uri($"{BaseUrl}/{query}");

                TwitterOAuthRequest request = new TwitterOAuthRequest();
                var rawResult = await request.EjecutarGetAsync(uri, _tokens);

                return parser.Parse(rawResult);
            }
            catch (WebException wex)
            {
                if (wex.Response is HttpWebResponse response)
                {
                    if ((int)response.StatusCode == 429)
                    {
                        throw new TooManyRequestsException();
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new OAuthKeysRevokedException();
                    }
                }

                throw;
            }
        }

        private async Task<bool> InitializeRequestAccessTokensAsync(string twitterCallbackUrl)
        {
            var twitterEnlace = $"{OAuthBaseUrl}/request_token";

            string nonce = GetNonce();
            string timeStamp = GetTimeStamp();
            string sigBaseStringParams = GetSignatureBaseStringParams(_tokens.ConsumerKey, nonce, timeStamp, "oauth_callback=" + Uri.EscapeDataString(twitterCallbackUrl));
            string sigBaseString = "GET&" + Uri.EscapeDataString(twitterEnlace) + "&" + Uri.EscapeDataString(sigBaseStringParams);
            string firma = GetSignature(sigBaseString, _tokens.ConsumerSecret);

            twitterEnlace += "?" + sigBaseStringParams + "&oauth_signature=" + Uri.EscapeDataString(firma);

            string getResponse;

            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(twitterEnlace)))
            {
                using (var response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    var data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        getResponse = data;
                    }
                    else
                    {
                        Debug.WriteLine("HttpHelper call failed trying to retrieve Twitter Request Tokens.  Message: {0}", data);
                        return false;
                    }
                }
            }

            var callbackConfirmed = ExtractTokenFromResponse(getResponse, TwitterOAuthTokenTipo.OAuthCallbackConfirmed);
            if (Convert.ToBoolean(callbackConfirmed) != true)
            {
                return false;
            }

            _tokens.RequestToken = ExtractTokenFromResponse(getResponse, TwitterOAuthTokenTipo.OAuthRequestOrAccessToken);
            _tokens.RequestTokenSecret = ExtractTokenFromResponse(getResponse, TwitterOAuthTokenTipo.OAuthRequestOrAccessTokenSecret);

            return true;
        }

        private string GetSignatureBaseStringParams(string consumerKey, string nonce, string timeStamp, string additionalParameters = "")
        {
            string sigBaseStringParams = additionalParameters;
            sigBaseStringParams += "&" + "oauth_consumer_key=" + consumerKey;
            sigBaseStringParams += "&" + "oauth_nonce=" + nonce;
            sigBaseStringParams += "&" + "oauth_signature_method=HMAC-SHA1";
            sigBaseStringParams += "&" + "oauth_timestamp=" + timeStamp;
            sigBaseStringParams += "&" + "oauth_version=1.0";

            return sigBaseStringParams;
        }

        private async Task<bool> ExchangeRequestTokenForAccessTokenAsync(string webAuthResultResponseData)
        {
            string responseData = webAuthResultResponseData.Substring(webAuthResultResponseData.IndexOf("oauth_token"));
            string requestToken = ExtractTokenFromResponse(responseData, TwitterOAuthTokenTipo.OAuthRequestOrAccessToken);

            if (requestToken != _tokens.RequestToken)
            {
                return false;
            }

            string oAuthVerifier = ExtractTokenFromResponse(responseData, TwitterOAuthTokenTipo.OAuthVerifier);

            string twitterUrl = $"{OAuthBaseUrl}/access_token";

            string timeStamp = GetTimeStamp();
            string nonce = GetNonce();

            string sigBaseStringParams = GetSignatureBaseStringParams(_tokens.ConsumerKey, nonce, timeStamp, "oauth_token=" + requestToken);

            string sigBaseString = "POST&";
            sigBaseString += Uri.EscapeDataString(twitterUrl) + "&" + Uri.EscapeDataString(sigBaseStringParams);

            string signature = GetSignature(sigBaseString, _tokens.ConsumerSecret);
            string data = null;

            string authorizationHeaderParams = "oauth_consumer_key=\"" + _tokens.ConsumerKey + "\", oauth_nonce=\"" + nonce + "\", oauth_signature_method=\"HMAC-SHA1\", oauth_signature=\"" + Uri.EscapeDataString(signature) + "\", oauth_timestamp=\"" + timeStamp + "\", oauth_token=\"" + Uri.EscapeDataString(requestToken) + "\", oauth_verifier=\"" + Uri.EscapeUriString(oAuthVerifier) + "\" , oauth_version=\"1.0\"";

            using (var request = new HttpRequestMessage(HttpMethod.Post, new Uri(twitterUrl)))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("OAuth", authorizationHeaderParams);

                using (var response = await _client.SendAsync(request).ConfigureAwait(false))
                {
                    data = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }

            var screenName = ExtractTokenFromResponse(data, TwitterOAuthTokenTipo.ScreenName);
            var accessToken = ExtractTokenFromResponse(data, TwitterOAuthTokenTipo.OAuthRequestOrAccessToken);
            var accessTokenSecret = ExtractTokenFromResponse(data, TwitterOAuthTokenTipo.OAuthRequestOrAccessTokenSecret);

            UsuarioScreenNombre = screenName;
            _tokens.AccessToken = accessToken;
            _tokens.AccessTokenSecret = accessTokenSecret;

            var passwordCredential = new PasswordCredential(screenName, accessToken, accessTokenSecret);
            //ApplicationData.Current.LocalSettings.Values["TwitterScreenNombre"] = screenName;
            boveda.Add(passwordCredential);

            return true;
        }

        private string GetNonce()
        {
            Random rand = new Random();
            int nonce = rand.Next(1000000000);
            return nonce.ToString();
        }

        private string GetTimeStamp()
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            return Math.Round(sinceEpoch.TotalSeconds).ToString();
        }

        private string GetSignature(string sigBaseString, string consumerSecretKey)
        {
            IBuffer keyMaterial = CryptographicBuffer.ConvertStringToBinary(consumerSecretKey + "&", BinaryStringEncoding.Utf8);
            MacAlgorithmProvider hmacSha1Provider = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            CryptographicKey macKey = hmacSha1Provider.CreateKey(keyMaterial);
            IBuffer dataToBeSigned = CryptographicBuffer.ConvertStringToBinary(sigBaseString, BinaryStringEncoding.Utf8);
            IBuffer signatureBuffer = CryptographicEngine.Sign(macKey, dataToBeSigned);
            string signature = CryptographicBuffer.EncodeToBase64String(signatureBuffer);

            return signature;
        }
    }
}