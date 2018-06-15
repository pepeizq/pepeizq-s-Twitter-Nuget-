using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pepeizq.Twitter.Stream;

namespace pepeizq.Twitter.OAuth
{
    public class TwitterOAuthRequest
    {
        private static HttpClient cliente;

        private bool _abort;

        public TwitterOAuthRequest()
        {
            if (cliente == null)
            {
                HttpClientHandler handler = new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip
                };

                cliente = new HttpClient(handler);
            }
        }

        public async Task<string> EjecutarGetAsync(Uri requestUri, TwitterOAuthTokens tokens)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                var requestBuilder = new TwitterOAuthRequestConstructor(requestUri, tokens, "GET");

                request.Headers.Authorization = AuthenticationHeaderValue.Parse(requestBuilder.AuthorizationHeader);

                using (var respuesta = await cliente.SendAsync(request).ConfigureAwait(false))
                {
                    return ProcesarErrores(await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
            }
        }

        public async Task EjecutarGetStreamAsync(Uri requestUri, TwitterOAuthTokens tokens, TwitterStreamLlamadas.RawJsonLlamada llamada)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                var requestBuilder = new TwitterOAuthRequestConstructor(requestUri, tokens);

                request.Headers.Authorization = AuthenticationHeaderValue.Parse(requestBuilder.AuthorizationHeader);

                using (var response = await cliente.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                    using (var reader = new StreamReader(responseStream))
                    {
                        while (!_abort && !reader.EndOfStream)
                        {
                            var result = reader.ReadLine();

                            if (!string.IsNullOrEmpty(result))
                            {
                                llamada?.Invoke(result);
                            }
                        }
                    }
                }
            }
        }

        public void Abortar()
        {
            _abort = true;
        }

        public async Task<string> EjecutarPostAsync(Uri requestUri, TwitterOAuthTokens tokens)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
            {
                var requestBuilder = new TwitterOAuthRequestConstructor(requestUri, tokens, "POST");

                request.Headers.Authorization = AuthenticationHeaderValue.Parse(requestBuilder.AuthorizationHeader);

                using (var response = await cliente.SendAsync(request).ConfigureAwait(false))
                {
                    return ProcesarErrores(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                }
            }
        }

        public async Task<string> ExecutePostMultipartAsync(Uri requestUri, TwitterOAuthTokens tokens, string boundary, byte[] content)
        {
            JToken mediaId = null;

            try
            {
                using (var multipartFormDataContent = new MultipartFormDataContent(boundary))
                {
                    using (var byteContent = new ByteArrayContent(content))
                    {
                        multipartFormDataContent.Add(byteContent, "media");

                        using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
                        {
                            var requestBuilder = new TwitterOAuthRequestConstructor(requestUri, tokens, "POST");

                            request.Headers.Authorization = AuthenticationHeaderValue.Parse(requestBuilder.AuthorizationHeader);

                            request.Content = multipartFormDataContent;

                            using (var response = await cliente.SendAsync(request).ConfigureAwait(false))
                            {
                                string jsonResultado = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                                JObject jObj = JObject.Parse(jsonResultado);
                                mediaId = jObj["media_id_string"];
                            }
                        }
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // http://stackoverflow.com/questions/39109060/httpmultipartformdatacontent-dispose-throws-objectdisposedexception
            }

            return mediaId.ToString();
        }

        private string ProcesarErrores(string content)
        {
            if (content.StartsWith("{\"errors\":"))
            {
                var errores = JsonConvert.DeserializeObject<TwitterErrores>(content);

                throw new TwitterExcepcion { Errores = errores };
            }

            return content;
        }
    }
}