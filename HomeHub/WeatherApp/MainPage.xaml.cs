using Openweathermap.net;
using System;
using Windows.UI.Xaml.Controls;

namespace WeatherApp
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var clock = new I2CLcd.TM1637Clock(24, 23);
            clock.ShowDigits(new byte[] { 1, 6, 3, 6 }, 0);
            

            //using (var client = new WeatherClient())
            //{
            //    var weather = await client.Request();
            //    txtTemp.Text = string.Format("{0:0.0}°C", weather.Air.Temperature);
            //    txtWeather.Text = weather.Main[0].Description;
            //    txtTime.Text = DateTime.Now.ToString("mm:HH");
            //}
        }
    }
}
