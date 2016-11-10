using System;
using System.Threading.Tasks;

namespace SensorBox
{
    interface IClient : IDisposable
    {
        bool IsConnected { get; }

        Task<bool> Connect();
    }
}
