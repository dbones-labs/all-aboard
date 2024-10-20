namespace AllAboard.Configuration
{
    using System;
    using Microsoft.Extensions.Hosting;


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
}
