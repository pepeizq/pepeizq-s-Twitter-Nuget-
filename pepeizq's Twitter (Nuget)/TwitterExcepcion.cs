using System;

namespace pepeizq.Twitter
{
    public class TwitterExcepcion : Exception
    {
        public TwitterErrores Errores { get; set; }
    }
}