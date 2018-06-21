using Microsoft.Toolkit.Parsers;
using pepeizq.Twitter.OAuth;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pepeizq.Twitter
{
    public class TwitterServicio
    {
        public TwitterDataProvider twitterDataProvider;

        public TwitterOAuthTokens tokens;

        private bool isInitialized;

        public TwitterServicio()
        {
        }

        private static TwitterServicio instancia;

        public static TwitterServicio Instance => instancia ?? (instancia = new TwitterServicio());

        public string UsuarioScreenNombre => Provider.UsuarioScreenNombre;

        public bool Iniciar(string consumerKey, string consumerSecret, string callbackUri)
        {
            if (string.IsNullOrEmpty(consumerKey))
            {
                throw new ArgumentNullException(nameof(consumerKey));
            }

            if (string.IsNullOrEmpty(consumerSecret))
            {
                throw new ArgumentNullException(nameof(consumerSecret));
            }

            if (string.IsNullOrEmpty(callbackUri))
            {
                throw new ArgumentNullException(nameof(callbackUri));
            }

            var oAuthTokens = new TwitterOAuthTokens
            {
                ConsumerKey = consumerKey,
                ConsumerSecret = consumerSecret,
                CallbackEnlace = callbackUri
            };

            return Iniciar(oAuthTokens);
        }

        public bool Iniciar(TwitterOAuthTokens oAuthTokens)
        {
            tokens = oAuthTokens ?? throw new ArgumentNullException(nameof(oAuthTokens));
            isInitialized = true;

            twitterDataProvider = null;

            return true;
        }

        public TwitterDataProvider Provider
        {
            get
            {
                if (!isInitialized)
                {
                    throw new InvalidOperationException("Provider no inicializado.");
                }

                return twitterDataProvider ?? (twitterDataProvider = new TwitterDataProvider(tokens));
            }
        }   

        public async Task<List<Tweet.Tweet>> RequestAsync(TwitterDataConfig config, int maxRecords = 20)
        {
            return await RequestAsync<Tweet.Tweet>(config, maxRecords);
        }

        public async Task<List<T>> RequestAsync<T>(TwitterDataConfig config, int maxRecords = 20)
            where T : SchemaBase
        {
            if (Provider.Logeado)
            {
                List<T> queryResults = new List<T>();

                var resultados = await Provider.LoadDataAsync<T>(config, maxRecords, 0, new TwitterParser<T>());

                foreach (var resultado in resultados)
                {
                    queryResults.Add(resultado);
                }

                return queryResults;
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await RequestAsync<T>(config, maxRecords);
            }

            return null;
        }

        public Task<bool> Logear()
        {
            return Provider.Logear();
        }

        public void Deslogear()
        {
            Provider.Deslogear();
        }
       
    }
}
