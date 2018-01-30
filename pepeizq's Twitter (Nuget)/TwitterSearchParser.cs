using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Services;
using Newtonsoft.Json;

namespace pepeizq.Twitter
{
    public class TwitterSearchParser : IParser<Tweet.Tweet>
    {
        public IEnumerable<Tweet.Tweet> Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            var resultado = JsonConvert.DeserializeObject<TwitterSearchResultado>(data);

            return resultado.Statuses.ToList();
        }
    }
}
