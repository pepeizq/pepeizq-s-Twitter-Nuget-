using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Toolkit.Services;
using Microsoft.Toolkit.Services.Exceptions;
using Newtonsoft.Json;
using pepeizq.Twitter.OAuth;
using pepeizq.Twitter.Stream;
using pepeizq.Twitter.Tweet;
using Windows.Security.Authentication.Web;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Streams;

namespace pepeizq.Twitter
{
    public class TwitterDataProvider : DataProviderBase<TwitterDataConfig, SchemaBase>
    {
        private const string BaseUrl = "https://api.twitter.com/1.1";
        private const string OAuthBaseUrl = "https://api.twitter.com/oauth";
        private const string PublishUrl = "https://upload.twitter.com/1.1";
        private const string UserStreamUrl = "https://userstream.twitter.com/1.1";

        private static HttpClient _client;

        private readonly TwitterOAuthTokens _tokens;

        private readonly PasswordVault _vault;

        private TwitterOAuthRequest _streamRequest;

        public string UserScreenName { get; set; }

        public bool LoggedIn { get; private set; }

        public TwitterDataProvider(TwitterOAuthTokens tokens)
        {
            _tokens = tokens;
            _vault = new PasswordVault();

            if (_client == null)
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.AutomaticDecompression = DecompressionMethods.GZip;
                _client = new HttpClient(handler);
            }
        }

        public async Task<TwitterUsuario> GetUserAsync(string screenName = null)
        {
            string rawResultado = null;
            try
            {
                var userScreenName = screenName ?? UserScreenName;
                var uri = new Uri($"{BaseUrl}/users/show.json?screen_name={userScreenName}");

                TwitterOAuthRequest request = new TwitterOAuthRequest();
                rawResultado = await request.EjecutarGetAsync(uri, _tokens);
                return JsonConvert.DeserializeObject<TwitterUsuario>(rawResultado);
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new UserNotFoundException(screenName);
                    }

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
            catch
            {
                if (!string.IsNullOrEmpty(rawResultado))
                {
                    var errores = JsonConvert.DeserializeObject<TwitterErrores>(rawResultado);

                    throw new TwitterExcepcion { Errores = errores };
                }

                throw;
            }
        }

        public async Task<IEnumerable<TSchema>> GetUserTimeLineAsync<TSchema>(string screenName, int maxRecords, IParser<TSchema> parser)
            where TSchema : SchemaBase
        {
            string rawResult = null;
            try
            {
                var uri = new Uri($"{BaseUrl}/statuses/user_timeline.json?screen_name={screenName}&count={maxRecords}&include_rts=1");

                TwitterOAuthRequest request = new TwitterOAuthRequest();
                rawResult = await request.EjecutarGetAsync(uri, _tokens);

                var result = parser.Parse(rawResult);
                return result
                        .Take(maxRecords)
                        .ToList();
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new UserNotFoundException(screenName);
                    }

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
            catch
            {
                if (!string.IsNullOrEmpty(rawResult))
                {
                    var errores = JsonConvert.DeserializeObject<TwitterErrores>(rawResult);

                    throw new TwitterExcepcion { Errores = errores };
                }

                throw;
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
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
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

        private PasswordCredential PasswordCredential
        {
            get
            {
                if (ApplicationData.Current.LocalSettings.Values["TwitterScreenName"] == null)
                {
                    return null;
                }

                var passwordCredentials = _vault.RetrieveAll();
                var temp = passwordCredentials.FirstOrDefault(c => c.Resource == "TwitterAccessToken");

                if (temp == null)
                {
                    return null;
                }

                return _vault.Retrieve(temp.Resource, temp.UserName);
            }
        }

        public async Task<bool> LoginAsync()
        {
            var twitterCredentials = PasswordCredential;
            if (twitterCredentials != null)
            {
                _tokens.AccessToken = twitterCredentials.UserName;
                _tokens.AccessTokenSecret = twitterCredentials.Password;
                UserScreenName = ApplicationData.Current.LocalSettings.Values["TwitterScreenName"].ToString();
                LoggedIn = true;
                return true;
            }

            if (await InitializeRequestAccessTokensAsync(_tokens.CallbackUri) == false)
            {
                LoggedIn = false;
                return false;
            }

            string requestToken = _tokens.RequestToken;
            string twitterUrl = $"{OAuthBaseUrl}/authorize?oauth_token={requestToken}";

            Uri startUri = new Uri(twitterUrl);
            Uri endUri = new Uri(_tokens.CallbackUri);

            var result = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startUri, endUri);

            switch (result.ResponseStatus)
            {
                case WebAuthenticationStatus.Success:
                    LoggedIn = true;
                    return await ExchangeRequestTokenForAccessTokenAsync(result.ResponseData);
                case WebAuthenticationStatus.ErrorHttp:
                    Debug.WriteLine("WAB failed, message={0}", result.ResponseErrorDetail.ToString());
                    LoggedIn = false;
                    return false;
                case WebAuthenticationStatus.UserCancel:
                    Debug.WriteLine("WAB user aborted.");
                    LoggedIn = false;
                    return false;
            }

            LoggedIn = false;
            return false;
        }

        public void Logout()
        {
            var twitterCredentials = PasswordCredential;
            if (twitterCredentials != null)
            {
                _vault.Remove(twitterCredentials);
                ApplicationData.Current.LocalSettings.Values["TwitterScreenName"] = null;
                UserScreenName = null;
            }

            LoggedIn = false;
        }

        public async Task<bool> RetweetStatusAsync(TwitterStatus status)
        {
            try
            {
                var uri = new Uri($"{BaseUrl}/statuses/retweet/{status.TweetID}.json");

                TwitterOAuthRequest request = new TwitterOAuthRequest();
                await request.EjecutarPostAsync(uri, _tokens);

                return true;
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
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

        public async Task<bool> DeshacerRetweetStatusAsync(TwitterStatus status)
        {
            try
            {
                var uri = new Uri($"{BaseUrl}/statuses/unretweet/{status.TweetID}.json");

                TwitterOAuthRequest request = new TwitterOAuthRequest();
                await request.EjecutarPostAsync(uri, _tokens);

                return true;
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
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

        public async Task<bool> TweetStatusAsync(string tweet, params IRandomAccessStream[] imagenes)
        {
            return await TweetStatusAsync(new TwitterStatus { Mensaje = tweet }, imagenes);
        }

        public async Task<bool> TweetStatusAsync(TwitterStatus status, params IRandomAccessStream[] imagenes)
        {
            try
            {
                var mediaIds = string.Empty;

                if (imagenes != null && imagenes.Length > 0)
                {
                    var ids = new List<string>();
                    foreach (var picture in imagenes)
                    {
                        ids.Add(await UploadPictureAsync(picture));
                    }

                    mediaIds = "&media_ids=" + string.Join(",", ids);
                }

                var uri = new Uri($"{BaseUrl}/statuses/update.json?{status.SolicitarParametrosRespuesta}{mediaIds}");

                TwitterOAuthRequest request = new TwitterOAuthRequest();
                await request.EjecutarPostAsync(uri, _tokens);

                return true;
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
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

        public Task StartUserStreamAsync(TwitterUsuarioStreamParser parser, TwitterStreamLlamadas.TwitterStreamLlamada llamada)
        {
            try
            {
                var uri = new Uri($"{UserStreamUrl}/user.json?replies=all");

                _streamRequest = new TwitterOAuthRequest();

                return _streamRequest.EjecutarGetStreamAsync(uri, _tokens, rawResult => llamada(parser.Parse(rawResult)));
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
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

        public void StopStream()
        {
            _streamRequest?.Abortar();
            _streamRequest = null;
        }

        public async Task<string> UploadPictureAsync(IRandomAccessStream stream)
        {
            var uri = new Uri($"{PublishUrl}/media/upload.json");

            var fileBytes = new byte[stream.Size];

            await stream.ReadAsync(fileBytes.AsBuffer(), (uint)stream.Size, InputStreamOptions.None);

            stream.Seek(0);

            string boundary = DateTime.Now.Ticks.ToString("x");

            TwitterOAuthRequest request = new TwitterOAuthRequest();
            return await request.ExecutePostMultipartAsync(uri, _tokens, boundary, fileBytes);
        }

        protected override IParser<SchemaBase> GetDefaultParser(TwitterDataConfig config)
        {
            if (config == null)
            {
                throw new ConfigNullException();
            }

            switch (config.QueryTipo)
            {
                case TwitterQueryTipo.Search:
                    return new TwitterSearchParser();
                case TwitterQueryTipo.Home:
                case TwitterQueryTipo.User:
                case TwitterQueryTipo.Custom:
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
                case TwitterQueryTipo.User:
                    items = await GetUserTimeLineAsync(config.Query, maxRecords, parser);
                    break;
                case TwitterQueryTipo.Search:
                    items = await SearchAsync(config.Query, maxRecords, parser);
                    break;
                case TwitterQueryTipo.Home:
                case TwitterQueryTipo.Custom:
                    items = await GetCustomSearch(config.Query, parser);
                    break;
                default:
                    items = await GetHomeTimeLineAsync(maxRecords, parser);
                    break;
            }

            return items;
        }

        protected override void ValidateConfig(TwitterDataConfig config)
        {
            if (config?.Query == null && config?.QueryTipo != TwitterQueryTipo.Home)
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

        private async Task<IEnumerable<TSchema>> GetHomeTimeLineAsync<TSchema>(int maxRecords, IParser<TSchema> parser)
            where TSchema : SchemaBase
        {
            try
            {
                var uri = new Uri($"{BaseUrl}/statuses/home_timeline.json?count={maxRecords}");

                TwitterOAuthRequest request = new TwitterOAuthRequest();
                var rawResult = await request.EjecutarGetAsync(uri, _tokens);

                return parser.Parse(rawResult);
            }
            catch (WebException wex)
            {
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
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
                HttpWebResponse response = wex.Response as HttpWebResponse;
                if (response != null)
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
            var twitterUrl = $"{OAuthBaseUrl}/request_token";

            string nonce = GetNonce();
            string timeStamp = GetTimeStamp();
            string sigBaseStringParams = GetSignatureBaseStringParams(_tokens.ConsumerKey, nonce, timeStamp, "oauth_callback=" + Uri.EscapeDataString(twitterCallbackUrl));
            string sigBaseString = "GET&" + Uri.EscapeDataString(twitterUrl) + "&" + Uri.EscapeDataString(sigBaseStringParams);
            string signature = GetSignature(sigBaseString, _tokens.ConsumerSecret);

            twitterUrl += "?" + sigBaseStringParams + "&oauth_signature=" + Uri.EscapeDataString(signature);

            string getResponse;

            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(twitterUrl)))
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

            UserScreenName = screenName;
            _tokens.AccessToken = accessToken;
            _tokens.AccessTokenSecret = accessTokenSecret;

            var passwordCredential = new PasswordCredential("TwitterAccessToken", accessToken, accessTokenSecret);
            ApplicationData.Current.LocalSettings.Values["TwitterScreenName"] = screenName;
            _vault.Add(passwordCredential);

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