
namespace Openweathermap.net.Http
{
    internal enum SiteCollection
    {
        [FieldValue("weather")]
        Current,
        [FieldValue("forecast")]
        HourForecast,
        [FieldValue("forecast/daily")]
        DailyForecast
    }
}
