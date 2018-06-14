using System.Collections.Generic;
using Microsoft.Toolkit.Parsers;
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