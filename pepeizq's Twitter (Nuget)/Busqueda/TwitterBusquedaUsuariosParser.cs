using System.Collections.Generic;
using Microsoft.Toolkit.Parsers;
using Newtonsoft.Json;

namespace pepeizq.Twitter.Busqueda
{
    public class TwitterBusquedaUsuariosParser : IParser<TwitterUsuario>
    {
        public IEnumerable<TwitterUsuario> Parse(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<List<TwitterUsuario>>(data);
        }
    }
}
