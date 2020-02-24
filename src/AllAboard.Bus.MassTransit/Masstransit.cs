namespace AllAboard.Bus.MassTransit
{
    using Configuration;
    using Integrations.Bus;
    using Microsoft.Extensions.DependencyInjection;
    using Services;


    public class Masstransit : IMessagingProvider {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IBusAdapter, BusAdapter>();
            services.AddSingleton<IIdStrategy, IdStrategy>();
        }
    }
}

