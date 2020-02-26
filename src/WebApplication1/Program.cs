namespace WebApplication1
{
    using System;
    using System.Linq;
    using System.Reflection;
    using AllAboard.Bus.MassTransit;
    using AllAboard.Configuration;
    using AllAboard.Data.Marten;
    using GreenPipes;
    using Infrastructure.Marten;
    using Infrastructure.MassTransit;
    using Marten;
    using MassTransit;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureAllAboard(setup =>
                {
                    setup.UseDataStore<Marten>();
                    setup.UseMessaging<MassTransit>();
                })

                .ConfigureServices(services =>
                {
                    var consumerTypes = Assembly
                        .GetEntryAssembly()
                        .ExportedTypes
                        .Where(x => !x.IsAbstract && typeof(IConsumer).IsAssignableFrom(x))
                        .ToList();

                    services.AddHostedService<BusService>();

                    

                    //SETUP Messaging
                    services.AddMassTransit(config =>
                    {
                        foreach (var consumerType in consumerTypes)
                        {
                            config.AddConsumer(consumerType);
                        }


                        config.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                        {

                            cfg.Host(new Uri($"rabbitmq://localhost"), h =>
                            {
                                h.Username("guest");
                                h.Password("guest");
                            });

                            cfg.ReceiveEndpoint("test-app", e =>
                            {
                                e.UseMessageRetry(x => x.Interval(10, new TimeSpan(0, 0, 0, 0, 500)));
                                e.ConfigureConsumer(provider, consumerTypes.ToArray());
                            });

                            cfg.UseServiceScope(provider);
                            cfg.UseAllAboard();
                        }));


                    });

                    //SET UP Database
                    services.AddSingleton<IDocumentStore>(s =>
                    {
                        var logger = s.GetService<ILogger<Program>>();

                        return DocumentStore.For(_ =>
                        {
                            _.Connection("host=localhost;port=6432;database=TestApp;password=ABC123;username=application");
                            //_.DatabaseSchemaName = dbConfig.Name;

                            _.CreateDatabasesForTenants(c =>
                            {
                                // Specify a db to which to connect in case database needs to be created.
                                // If not specified, defaults to 'postgres' on the connection for a tenant.
                                //c.MaintenanceDatabase(cstring);
                                c.ForTenant()
                                    .CheckAgainstPgDatabase()
                                    .WithOwner("application")
                                    .WithEncoding("UTF-8")
                                    .ConnectionLimit(-1)
                                    .OnDatabaseCreated(__ =>
                                    {
                                        logger.LogInformation($"created {__.Database}");
                                        //dbCreated = true;
                                    });
                            });

                            _.DefaultIdStrategy = (mapping, storeOptions) => new StringIdGeneration();
                            _.AutoCreateSchemaObjects = AutoCreate.All;
                        });
                    });

                    services.AddScoped(s => s.GetService<IDocumentStore>().DirtyTrackedSession());



                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
