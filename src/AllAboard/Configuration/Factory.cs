namespace AllAboard.Configuration
{
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using Services.Background;
    using Services.Consuming;

    public class Factory : IFactory
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddHostedService<BusHostedService>();
            services.AddScoped<IBus, TransactionBus>();
            services.AddScoped<MessageQueuing>();
            services.AddScoped<MessageFilter>();

            services.AddSingleton<IScheduler, DefaultScheduler>();
            services.AddScoped<MessageProcessor>();

            services.AddScoped<PublishMessageContext>();
            services.AddScoped<ConsumingMessageContext>();
        }
    }
}