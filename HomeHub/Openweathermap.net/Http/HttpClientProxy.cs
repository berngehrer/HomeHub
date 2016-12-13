using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Openweathermap.net.Http
{
    internal sealed class HttpClientProxy : IDisposable
    {
        HttpClient _innerClient;

        public HttpClientProxy()
        {
            _innerClient = CreateClient();
        }

        public async Task<string> ReadAsString(MessageHandler message)
        {
            using (var response = await GetResponse(message))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        Task<HttpResponseMessage> GetResponse(MessageHandler message)
        {
            var header = new MediaTypeWithQualityHeaderValue(UrlProvider.MimeString);
            var request = new HttpRequestMessage(HttpMethod.Get, message.GetRelative());

            if (message.CancelToken.HasValue) {
                return _innerClient.SendAsync(request, message.CancelToken.Value);
            }
            else {
                return _innerClient.SendAsync(request);
            }
        }

        HttpClient CreateClient()
        {
            var httpHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
            return new HttpClient(httpHandler, true)
            {
                BaseAddress = new Uri(UrlProvider.HostAddress)
            };
        }

        public void Dispose()
        {
            _innerClient?.CancelPendingRequests();
            _innerClient?.Dispose();
        }
    }
}
