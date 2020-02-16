using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1
{
    using System.Reflection;
    using System.Threading;
    using AllAboard.Bus.MassTransit;
    using MassTransit;
    using Microsoft.AspNetCore.Http;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureAllAbord(host =>
                {
                    host.Use<Marten>();
                    host.Use<Masstransit>();
                })

                .ConfigureServices(services =>
                {

                    //healthcheck part 1

                    var consumerTypes = Assembly
                        .GetEntryAssembly()
                        .ExportedTypes
                        .Where(x => !x.IsAbstract && typeof(IConsumer).IsAssignableFrom(x))
                        .ToList();

                    services.AddHostedService<BusService>();

                    


                    services.AddMassTransit(config =>
                    {
                        foreach (var consumerType in consumerTypes)
                        {
                            config.AddConsumer(consumerType);
                        }

                        config.AddBus(provider => Bus.Factory.CreateUsingInMemory(cfg =>
                        {
                            cfg.ReceiveEndpoint("queue", e =>
                            {
                                e.ConfigureConsumer(provider, consumerTypes.ToArray());
                            });

                            cfg.UseServiceScope(provider);
                            cfg.UseAllAboard();
                        }));


                    });


                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }



    

    public class BusService :
        IHostedService
    {
        private readonly IBusControl _busControl;

        public BusService(IBusControl busControl)
        {
            _busControl = busControl;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return _busControl.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return _busControl.StopAsync(cancellationToken);
        }
    }
}
