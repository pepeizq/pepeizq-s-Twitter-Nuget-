using System.Collections.Generic;
using Newtonsoft.Json;

namespace pepeizq.Twitter.Busqueda
{
    public class TwitterBusquedaUsuariosParser 
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
