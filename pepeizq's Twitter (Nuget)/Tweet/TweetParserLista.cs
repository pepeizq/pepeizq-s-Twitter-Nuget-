using System.Collections.Generic;
using Microsoft.Toolkit.Services;
using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetParser : IParser<Tweet>
    {
        public IEnumerable<Tweet> Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<Tweet>>(data);
        }
    }
}