namespace AllAboard.Configuration
{
    using System;
    using Integrations.Data;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Services;


    public static class HostExtensions
    {
       
        public static IHostBuilder ConfigureAllAboard(this IHostBuilder builder, Action<HostSetup> conf)
        {
            var setup = new HostSetup();
            conf(setup);

            builder.ConfigureServices((ctx, services) =>
            {
                var baseFactory = new Factory();
                baseFactory.RegisterServices(services);
                
                setup.DataStoreProvider.RegisterServices(services);
                setup.MessagingProvider.RegisterServices(services);
            });

            return builder;
        }
    }

    public class HostSetup
    {
        internal IFactory DataStoreProvider { get; set; }
        internal IFactory MessagingProvider { get; set; }

        public void UseDataStore<T>() where T : IDataStoreProvider, new()
        {
            DataStoreProvider = new T();
        }

        public void UseMessaging<T>() where T : IMessagingProvider, new()
        {
            MessagingProvider = new T();
        }
    }

  

    public interface IFactory
    {
        void RegisterServices(IServiceCollection services);
    }

    public interface  IDataStoreProvider : IFactory { }
    public interface IMessagingProvider : IFactory { }


    public class InMemoryDatabaseFactory : IFactory
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<MemoryStore>();
            services.AddScoped<ISession, MemorySession>();
        }
    }


    public class Factory : IFactory
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddHostedService<BusHostedService>();
            services.AddScoped<IBus, TransactionBus>();
            services.AddScoped<BackgroundMessageQueuing>();
            services.AddScoped<MessageFilter>();

            services.AddSingleton<IScheduler, BackgroundScheduler>();
            services.AddScoped<MessageProcessor>();

            services.AddScoped<PublishMessageContext>();
            services.AddScoped<ConsumingMessageContext>();
        }
    }
}
