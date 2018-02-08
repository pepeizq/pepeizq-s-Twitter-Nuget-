using Newtonsoft.Json;

namespace pepeizq.Twitter.Tweet
{
    public class TweetParserIndividual : Tweet
    {
        public Tweet Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Tweet>(data);
        }
    }
}