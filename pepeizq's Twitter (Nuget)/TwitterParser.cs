using System.Collections.Generic;
using Microsoft.Toolkit.Services;
using Newtonsoft.Json;

namespace pepeizq.Twitter
{
    public class TwitterParser<T> : IParser<T>
        where T : SchemaBase
    {
        IEnumerable<T> IParser<T>.Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<List<T>>(data);
            }
            catch (JsonSerializationException)
            {
                List<T> items = new List<T>();
                items.Add(JsonConvert.DeserializeObject<T>(data));
                return items;
            }
        }
    }
}
