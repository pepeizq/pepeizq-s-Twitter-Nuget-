using Newtonsoft.Json;

namespace pepeizq.Twitter.OEmbed
{
    public class OEmbedParser : OEmbed
    {
        public OEmbed Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<OEmbed>(data);
        }
    }
}
