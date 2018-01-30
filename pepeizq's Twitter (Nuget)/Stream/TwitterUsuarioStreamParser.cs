using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace pepeizq.Twitter.Stream
{
    public class TwitterUsuarioStreamParser
    {
        public ITwitterResultado Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            var obj = (JObject)JsonConvert.DeserializeObject(data);

            var friends = obj.SelectToken("friends", false);
            if (friends != null && friends.HasValues)
            {
                return null;
            }

            var delete = obj.SelectToken("delete", false);
            if (delete != null)
            {
                var deletedStatus = delete.SelectToken("status", false);
                if (deletedStatus != null && deletedStatus.HasValues)
                {
                    return JsonConvert.DeserializeObject<TwitterStreamEventoBorrar>(deletedStatus.ToString());
                }

                var deletedDirectMessage = delete.SelectToken("direct_message", false);
                if (deletedDirectMessage != null && deletedDirectMessage.HasValues)
                {
                    return JsonConvert.DeserializeObject<TwitterStreamEventoBorrar>(deletedDirectMessage.ToString());
                }
            }

            var events = obj.SelectToken("event", false);
            if (events != null)
            {
                var targetobject = obj.SelectToken("target_object", false);
                Tweet.Tweet endtargetobject = null;
                if (targetobject?.SelectToken("user", false) != null)
                {
                    endtargetobject = JsonConvert.DeserializeObject<Tweet.Tweet>(targetobject.ToString());
                }

                var endevent = JsonConvert.DeserializeObject<TwitterStreamEvento>(obj.ToString());
                endevent.ObjetivoTweet = endtargetobject;
                return endevent;
            }

            var user = obj.SelectToken("user", false);
            if (user != null && user.HasValues)
            {
                return JsonConvert.DeserializeObject<Tweet.Tweet>(obj.ToString());
            }

            var directMessage = obj.SelectToken("direct_message", false);
            if (directMessage != null && directMessage.HasValues)
            {
                return JsonConvert.DeserializeObject<TwitterMensajeDirecto>(directMessage.ToString());
            }

            return null;
        }
    }
}
