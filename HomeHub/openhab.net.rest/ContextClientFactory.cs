﻿using openhab.net.rest.Core;
using System;

namespace openhab.net.rest
{
    internal class ContextClientFactory 
    {
        public ContextClientFactory(OpenhabSettings settings, UpdateStrategy strategy)
        {
            Settings = settings;
            Strategy = strategy ?? UpdateStrategy.Default;
        }

        public OpenhabSettings Settings { get; }
        public UpdateStrategy Strategy { get; }

        public OpenhabClient CreateClient()
        {
            return new OpenhabClient(Settings);
        }

        public BackgroundClient CreateWorker()
        {
            var workerClient = Create(true);
            if (workerClient != null) {
                return new BackgroundClient(workerClient, (int)Strategy.Interval.TotalMilliseconds);
            }
            return null;
        }

        OpenhabClient Create(bool withStrategy)
        {
            var strategy = withStrategy ? this.Strategy : UpdateStrategy.Default;

            // Timed update
            if (strategy.Interval != TimeSpan.Zero)
            {
                return new OpenhabClient(Settings);
            }
            // Permanent update by server push
            else if (strategy.Realtime)
            {
                return new OpenhabClient(Settings, pooling: true);
            }
            // No background update
            return null;
        }
    }
}
