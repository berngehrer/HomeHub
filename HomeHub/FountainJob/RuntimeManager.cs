using openhab.net.rest;
using openhab.net.rest.Items;
using System;
using System.Threading;

namespace FountainJob
{
    class RuntimeManager
    {
        ItemContext _items;
        SwitchItem _fountain;
        DelayTimer _pingTimer;
        CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public async void Start()
        {
            _items = new ItemContext(Constants.Host);
            
            _fountain = await _items.GetByName<SwitchItem>(Constants.Item, _tokenSource.Token);
            _fountain.Changed += Fountain_Changed;

            _pingTimer = new DelayTimer(Constants.PingDelay);
            _pingTimer.Start(PingSlave);
        }

        async void Fountain_Changed(object sender, EventArgs e)
        {
            using (var client = new I2CClient(Constants.SLAVE_ADDRESS))
            {
                if (await client.Connect())
                {
                    client.SetModus(_fountain.Value == true ? Modus.ON : Modus.OFF);
                }
            }
        }

        async void PingSlave()
        {
            using (var client = new I2CClient(Constants.SLAVE_ADDRESS))
            {
                if (await client.Connect())
                {
                    client.Ping();
                }
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel(false);
            _pingTimer.Dispose();
            _items.Dispose();
        }
    }
}
