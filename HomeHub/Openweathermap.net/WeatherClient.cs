using Openweathermap.net.Http;
using System.Threading.Tasks;
using System;
using Openweathermap.net.Json;
using Newtonsoft.Json;

namespace Openweathermap.net
{
    public sealed class WeatherClient : System.IDisposable
    {
        HttpClientProxy _proxy = new HttpClientProxy();

        public WeatherClient()
        {
        }
        
        public async Task<CurrentWeatherObject> Request()
        {
            var json = await _proxy.ReadAsString(new MessageHandler
            {
                Collection = SiteCollection.Current
            });
            return JsonConvert.DeserializeObject<CurrentWeatherObject>(json);
        }

        public void Dispose()
        {
            _proxy.Dispose();
        }
    }
}
