using Microsoft.Toolkit.Services;
using pepeizq.Twitter.Banner;
using pepeizq.Twitter.Busqueda;
using pepeizq.Twitter.OAuth;
using pepeizq.Twitter.OEmbed;
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

        private static TwitterServicio instancia;

        public static TwitterServicio Instance => instancia ?? (instancia = new TwitterServicio());

        public string UsuarioScreenNombre => Provider.UsuarioScreenNombre;

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
            if (Provider.Logeado)
            {
                return await Provider.SearchAsync(hashTag, maxRecords, new TwitterBusquedaTweetsParser());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await SearchAsync(hashTag, maxRecords);
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

        //--------------------------------------------

        public async Task<TwitterUsuario> GenerarUsuario(string screenName = null)
        {
            if (Provider.Logeado)
            {
                return await Provider.GenerarUsuario(screenName);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await GenerarUsuario(screenName);
            }

            return null;
        }

        public async Task<IEnumerable<Tweet.Tweet>> CogerTweetsTimelineInicio(TwitterOAuthTokens tokens, string ultimoTweet)
        {
            if (Provider.Logeado)
            {
                return await Provider.CogerTweetsTimelineInicio(tokens,ultimoTweet, new TweetParser());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await CogerTweetsTimelineInicio(tokens,ultimoTweet);
            }

            return null;
        }

        public async Task<IEnumerable<Tweet.Tweet>> CogerTweetsTimelineMenciones(TwitterOAuthTokens tokens, string ultimoTweet)
        {
            if (Provider.Logeado)
            {
                return await Provider.CogerTweetsTimelineMenciones(tokens,ultimoTweet, new TweetParser());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await CogerTweetsTimelineMenciones(tokens,ultimoTweet);
            }

            return null;
        }

        public async Task<IEnumerable<Tweet.Tweet>> CogerTweetsTimelineUsuario(string screenNombre, string ultimoTweet)
        {
            if (Provider.Logeado)
            {
                return await Provider.CogerTweetsTimelineUsuario(screenNombre, ultimoTweet, new TweetParser());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await CogerTweetsTimelineUsuario(screenNombre, ultimoTweet);
            }

            return null;
        }

        public async Task<Tweet.Tweet> CogerTweet(TwitterOAuthTokens tokens, string idTweet)
        {
            if (Provider.Logeado)
            {
                return await Provider.CogerTweet(tokens, idTweet, new TweetParserIndividual());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await CogerTweet(tokens, idTweet);
            }

            return null;
        }

        public async Task<IEnumerable<Tweet.Tweet>> BuscarRespuestasTweet(TwitterOAuthTokens tokens, string screenNombre, string tweetID)
        {
            if (Provider.Logeado)
            {
                return await Provider.BuscarRespuestasTweet(tokens, screenNombre, tweetID, new TwitterBusquedaTweetsParser());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await BuscarRespuestasTweet(tokens, screenNombre, tweetID);
            }

            return null;
        }

        public async Task<IEnumerable<TwitterUsuario>> BuscarUsuarios(TwitterOAuthTokens tokens, string screenNombre)
        {
            if (Provider.Logeado)
            {
                return await Provider.BuscarUsuarios(tokens, screenNombre, new TwitterBusquedaUsuariosParser());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await BuscarUsuarios(tokens, screenNombre);
            }

            return null;
        }

        public async Task<Banner.Banner> CogerBannerUsuario(string screenNombre)
        {
            if (Provider.Logeado)
            {
                return await Provider.CogerBannerUsuario(screenNombre, new BannerParser());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await CogerBannerUsuario(screenNombre);
            }

            return null;
        }

        public async Task<OEmbed.OEmbed> CogerOEmbedTweet(string enlace)
        {
            if (Provider.Logeado)
            {
                return await Provider.CogerOEmbedTweet(enlace, new OEmbedParser());
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await CogerOEmbedTweet(enlace);
            }

            return null;
        }

        public async Task<bool> Favoritear(TwitterOAuthTokens tokens, TwitterStatus status)
        {
            if (Provider.Logeado)
            {
                return await Provider.Favoritear(tokens,status);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await Favoritear(tokens,status);
            }

            return false;
        }

        public async Task<bool> DeshacerFavorito(TwitterOAuthTokens tokens, TwitterStatus status)
        {
            if (Provider.Logeado)
            {
                return await Provider.DeshacerFavorito(tokens,status);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await DeshacerFavorito(tokens,status);
            }

            return false;
        }

        public async Task<bool> Retwitear(TwitterOAuthTokens tokens, TwitterStatus status)
        {
            if (Provider.Logeado)
            {
                return await Provider.Retwitear(tokens, status);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await Retwitear(tokens,status);
            }

            return false;
        }

        public async Task<bool> DeshacerRetweet(TwitterOAuthTokens tokens, TwitterStatus status)
        {
            if (Provider.Logeado)
            {
                return await Provider.DeshacerRetweet(tokens,status);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await DeshacerRetweet(tokens,status);
            }

            return false;
        }

        public async Task<bool> BloquearUsuario(TwitterOAuthTokens tokens, string screenNombre)
        {
            if (Provider.Logeado)
            {
                return await Provider.BloquearUsuario(tokens, screenNombre);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await BloquearUsuario(tokens, screenNombre);
            }

            return false;
        }

        public async Task<bool> DeshacerBloquearUsuario(TwitterOAuthTokens tokens, string screenNombre)
        {
            if (Provider.Logeado)
            {
                return await Provider.DeshacerBloquearUsuario(tokens, screenNombre);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await DeshacerBloquearUsuario(tokens, screenNombre);
            }

            return false;
        }

        public async Task<bool> ReportarUsuario(TwitterOAuthTokens tokens, string screenNombre)
        {
            if (Provider.Logeado)
            {
                return await Provider.ReportarUsuario(tokens, screenNombre);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await ReportarUsuario(tokens, screenNombre);
            }

            return false;
        }

        public async Task<bool> MutearUsuario(TwitterOAuthTokens tokens, string screenNombre)
        {
            if (Provider.Logeado)
            {
                return await Provider.MutearUsuario(tokens, screenNombre);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await MutearUsuario(tokens, screenNombre);
            }

            return false;
        }

        public async Task<bool> DeshacerMutearUsuario(TwitterOAuthTokens tokens, string screenNombre)
        {
            if (Provider.Logeado)
            {
                return await Provider.DeshacerMutearUsuario(tokens, screenNombre);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await DeshacerMutearUsuario(tokens, screenNombre);
            }

            return false;
        }

        public async Task<bool> EnviarTweet(TwitterOAuthTokens tokens, string mensaje, params IRandomAccessStream[] pictures)
        {
            return await EnviarTweet(tokens, new TwitterStatus { Mensaje = mensaje }, pictures);
        }

        public async Task<bool> EnviarTweet(TwitterOAuthTokens tokens, TwitterStatus status, params IRandomAccessStream[] pictures)
        {
            if (pictures.Length > 4)
            {
                throw new ArgumentOutOfRangeException(nameof(pictures));
            }

            if (Provider.Logeado)
            {
                return await Provider.EnviarTweet(tokens,status, pictures);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await EnviarTweet(tokens,status, pictures);
            }

            return false;
        }

        public async Task<bool> SeguirUsuario(TwitterOAuthTokens tokens, string usuarioID)
        {
            if (Provider.Logeado)
            {
                return await Provider.SeguirUsuario(tokens, usuarioID);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await SeguirUsuario(tokens, usuarioID);
            }

            return false;
        }

        public async Task<bool> DeshacerSeguirUsuario(TwitterOAuthTokens tokens, string usuarioID)
        {
            if (Provider.Logeado)
            {
                return await Provider.DeshacerSeguirUsuario(tokens, usuarioID);
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                return await DeshacerSeguirUsuario(tokens, usuarioID);
            }

            return false;
        }

        public async Task ArrancarStreamUsuario(TwitterOAuthTokens tokens, TwitterStreamLlamadas.TwitterStreamLlamada llamada)
        {
            if (Provider.Logeado)
            {
                await Provider.ArrancarStreamUsuario(tokens,new TwitterUsuarioStreamParser(), llamada);
                return;
            }

            var isLoggedIn = await Logear();
            if (isLoggedIn)
            {
                await ArrancarStreamUsuario(tokens,llamada);
            }
        }

        public void PararStreamUsuario()
        {
            Provider.PararStreamUsuario();
        }
    }
}
