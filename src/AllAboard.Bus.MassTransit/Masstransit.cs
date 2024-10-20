﻿namespace AllAboard.Bus.MassTransit
{
    using Configuration;
    using Integrations.Bus;
    using Microsoft.Extensions.DependencyInjection;

    public class MassTransit : IMessagingProvider {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IBusAdapter, BusAdapter>();
            services.AddSingleton<IIdStrategy, IdStrategy>();
        }
    }
}

