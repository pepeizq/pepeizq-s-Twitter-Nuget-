using Newtonsoft.Json;

namespace pepeizq.Twitter.Banner
{
    public class BannerParser : Banner
    {
        public Banner Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Banner>(data);
        }
    }
}