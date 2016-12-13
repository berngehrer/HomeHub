
namespace Openweathermap.net.Http
{
    internal static class UrlProvider
    {
        public static string HostAddress = @"http://api.openweathermap.org";
        public static string MimeString  = @"application/json";
        
        const string AppId      = "4f6a3ce832940a448a6eb191a3ed7cda";
        const string CityId     = "6556201";
        const string Mode       = "json";
        const string Units      = "metric";
        const string Language   = "de";

        public static string Build(SiteCollection collection, uint? cnt)
        {
            var relative = $"data/2.5/{collection.GetValue()}?id={CityId}&mode={Mode}&units={Units}&lang={Language}&APPID={AppId}";
            if (cnt.HasValue && collection != SiteCollection.Current)
            {
                relative += $"&cnt={cnt.Value}";
            }
            return relative;
        }
        
    }
}
