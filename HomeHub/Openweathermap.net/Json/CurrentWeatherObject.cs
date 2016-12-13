using Newtonsoft.Json;
using System.Collections.Generic;

namespace Openweathermap.net.Json
{
    [JsonObject]
    public class CurrentWeatherObject
    {
        [JsonProperty("weather")]
        public List<WeatherObject> Main { get; set; }
        [JsonProperty("main")]
        public AirObject Air { get; set; }
        [JsonProperty("wind")]
        public WindObject Wind { get; set; }
        [JsonProperty("sys")]
        public SunObject Sun { get; set; }
        [JsonProperty("dt")]
        public double Timestamp { get; set; }
    }

    public class AirObject
    {
        [JsonProperty("temp")]
        public float Temperature { get; set; }
        [JsonProperty("pressure")]
        public float Pressure { get; set; }
        [JsonProperty("humidity")]
        public float Humidity { get; set; }
    }

    public class WeatherObject
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class WindObject
    {
        [JsonProperty("speed")]
        public float Speed { get; set; }
    }

    public class SunObject
    {
        [JsonProperty("sunrise")]
        public double Sunrise { get; set; }
        [JsonProperty("sunset")]
        public double Sunset { get; set; }
    }
}
