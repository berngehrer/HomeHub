using Newtonsoft.Json;
using openhab.net.rest.Core;
using openhab.net.rest.Http;
using System;
using System.Threading.Tasks;

namespace openhab.net.rest
{
    internal class OpenhabClient : IDisposable
    {
        HttpClientProxy _proxy;

        public OpenhabClient(OpenhabSettings settings, bool pooling = false)
        {
            Settings = settings;

            var poolingClass = PoolingSession.False;
            if (pooling) {
                poolingClass = new PoolingSession(IdProvider.GetNext());
            }
            _proxy = new HttpClientProxy(settings, poolingClass);
        }

        public OpenhabSettings Settings { get; }


        public async Task<bool> SendCommand(MessageHandler message)
        {
            return await _proxy.SendMessage(message);
        }

        public async Task<T> SendRequest<T>(MessageHandler message) where T : class, new()
        {
            var json = await _proxy.ReadAsString(message);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public void Dispose()
        {
            _proxy.Dispose();
        }
    }
}
