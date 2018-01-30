namespace pepeizq.Twitter.Stream
{
    public class TwitterStreamLlamadas
    {
        public delegate void RawJsonLlamada(string json);

        public delegate void TwitterStreamLlamada(ITwitterResultado tweet);
    }
}
