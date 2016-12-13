using System.Threading;

namespace Openweathermap.net.Http
{
    internal class MessageHandler
    {
        public uint? Count { get; set; }
        public SiteCollection Collection { get; set; }
        public CancellationToken? CancelToken { get; set; }

        public string GetRelative() => UrlProvider.Build(Collection, Count);
    }
}
