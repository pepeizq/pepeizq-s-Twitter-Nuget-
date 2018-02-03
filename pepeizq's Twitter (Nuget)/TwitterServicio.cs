using Microsoft.Toolkit.Services;
using pepeizq.Twitter.OAuth;
using pepeizq.Twitter.Stream;
using pepeizq.Twitter.Tweet;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace pepeizq.Twitter
{
    public class TwitterServicio
    {
        private TwitterDataProvider twitterDataProvider;

        private TwitterOAuthTokens tokens;

        private bool isInitialized;

        public TwitterServicio()
        {
        }

        private static TwitterServicio instance;

        public static TwitterServicio Instance => instance ?? (instance = new TwitterServicio());

        public string UserScreenName => Provider.UserScreenName;

        public bool Initialize(string consumerKey, string consumerSecret, string callbackUri)
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
                CallbackUri = callbackUri
            };

            return Initialize(oAuthTokens);
        }

        public bool Initialize(TwitterOAuthTokens oAuthTokens)
        {
            if (oAuthTokens == null)
            {
                throw new ArgumentNullException(nameof(oAuthTokens));
            }

            tokens = oAuthTokens;
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
                    throw new InvalidOperationException("Provider not initialized.");
                }

                return twitterDataProvider ?? (twitterDataProvider = new TwitterDataProvider(tokens));
            }
        }

        public async Task<IEnumerable<Tweet.Tweet>> SearchAsync(string hashTag, int maxRecords = 20)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.SearchAsync(hashTag, maxRecords, new TwitterSearchParser());
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await SearchAsync(hashTag, maxRecords);
            }

            return null;
        }

        public async Task<TwitterUsuario> GetUserAsync(string screenName = null)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.GetUserAsync(screenName);
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await GetUserAsync(screenName);
            }

            return null;
        }

       

        public async Task<List<Tweet.Tweet>> RequestAsync(TwitterDataConfig config, int maxRecords = 20)
        {
            return await RequestAsync<Tweet.Tweet>(config, maxRecords);
        }

        public async Task<List<T>> RequestAsync<T>(TwitterDataConfig config, int maxRecords = 20)
            where T : SchemaBase
        {
            if (Provider.LoggedIn)
            {
                List<T> queryResults = new List<T>();

                var resultados = await Provider.LoadDataAsync<T>(config, maxRecords, 0, new TwitterParser<T>());

                foreach (var resultado in resultados)
                {
                    queryResults.Add(resultado);
                }

                return queryResults;
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await RequestAsync<T>(config, maxRecords);
            }

            return null;
        }

        public Task<bool> LoginAsync()
        {
            return Provider.LoginAsync();
        }

        public void Logout()
        {
            Provider.Logout();
        }

        //--------------------------------------------

        public async Task<IEnumerable<Tweet.Tweet>> CogerTweetsTimelineInicio(string ultimoTweet)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.CogerTweetsTimelineInicio(ultimoTweet, new TweetParser());
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await CogerTweetsTimelineInicio(ultimoTweet);
            }

            return null;
        }

        public async Task<IEnumerable<Tweet.Tweet>> CogerTweetsTimelineMenciones(string ultimoTweet)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.CogerTweetsTimelineMenciones(ultimoTweet, new TweetParser());
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await CogerTweetsTimelineMenciones(ultimoTweet);
            }

            return null;
        }

        public async Task<IEnumerable<Tweet.Tweet>> CogerTweetsTimelineUsuario(string screenNombre, string ultimoTweet)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.CogerTweetsTimelineUsuario(screenNombre, ultimoTweet, new TweetParser());
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await CogerTweetsTimelineUsuario(screenNombre, ultimoTweet);
            }

            return null;
        }

        public async Task<bool> Favoritear(TwitterStatus status)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.Favoritear(status);
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await Favoritear(status);
            }

            return false;
        }

        public async Task<bool> DeshacerFavorito(TwitterStatus status)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.DeshacerFavorito(status);
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await DeshacerFavorito(status);
            }

            return false;
        }

        public async Task<bool> Retwitear(TwitterStatus status)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.Retwitear(status);
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await Retwitear(status);
            }

            return false;
        }

        public async Task<bool> DeshacerRetweet(TwitterStatus status)
        {
            if (Provider.LoggedIn)
            {
                return await Provider.DeshacerRetweet(status);
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await DeshacerRetweet(status);
            }

            return false;
        }

        public async Task<bool> EnviarTweet(string mensaje, params IRandomAccessStream[] pictures)
        {
            return await EnviarTweet(new TwitterStatus { Mensaje = mensaje }, pictures);
        }

        public async Task<bool> EnviarTweet(TwitterStatus status, params IRandomAccessStream[] pictures)
        {
            if (pictures.Length > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(pictures));
            }

            if (Provider.LoggedIn)
            {
                return await Provider.EnviarTweet(status, pictures);
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                return await EnviarTweet(status, pictures);
            }

            return false;
        }

        public async Task ArrancarStreamUsuario(TwitterStreamLlamadas.TwitterStreamLlamada llamada)
        {
            if (Provider.LoggedIn)
            {
                await Provider.ArrancarStreamUsuario(new TwitterUsuarioStreamParser(), llamada);
                return;
            }

            var isLoggedIn = await LoginAsync();
            if (isLoggedIn)
            {
                await ArrancarStreamUsuario(llamada);
            }
        }

        public void PararStreamUsuario()
        {
            Provider.PararStreamUsuario();
        }
    }
}
