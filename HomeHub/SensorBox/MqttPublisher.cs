using System;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace SensorBox
{
    class MqttPublisher : IClient
    {
        MqttClient _innerClient;

        public MqttPublisher(string host)
        {
            _innerClient = new MqttClient(host);
        }

        public bool IsConnected => _innerClient.IsConnected;


        public async Task<bool> Connect()
        {
            try {
                _innerClient.Connect( GetHostName() );
                await Task.Delay(3000);
                return IsConnected;
            }
            catch { return false; }
        }

        public async Task Send(string topic, string msg)
        {
            _innerClient.Publish(topic, Encoding.UTF8.GetBytes(msg));
            await Task.Delay(2000);
        }

        string GetHostName()
        {
            foreach (HostName name in NetworkInformation.GetHostNames())
            {
                if (HostNameType.DomainName == name.Type) {
                    return name.DisplayName;
                }
            }
            return "RPi3Win";
        }

        public void Dispose()
        {
            _innerClient.Disconnect();
            _innerClient = null;
        }
    }
}
