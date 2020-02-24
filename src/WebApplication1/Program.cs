namespace WebApplication1
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using AllAboard.Bus.MassTransit;
    using AllAboard.Configuration;
    using AllAboard.Data.Marten;
    using AllAboard.Services;
    using Marten;
    using Marten.Schema.Identity;
    using Marten.Storage;
    using MassTransit;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
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
                    setup.UseMessaging<Masstransit>();
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

                        config.AddBus(provider => Bus.Factory.CreateUsingInMemory(cfg =>
                        {
                            cfg.ReceiveEndpoint("queue", e =>
                            {
                                e.ConfigureConsumer(provider, consumerTypes.ToArray());
                            });

                            cfg.UseServiceScope(provider);

                            //add to the pipeline
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


    public class StringIdGeneration : IIdGeneration
    {

        public IEnumerable<System.Type> KeyTypes { get; } = new System.Type[] { typeof(string) };

        public IIdGenerator<T> Build<T>()
        {
            return (IIdGenerator<T>)new StringIdGenerator();
        }

        public bool RequiresSequences { get; } = false;

        public class StringIdGenerator : IIdGenerator<string>
        {
            public string Assign(ITenant tenant, string existing, out bool assigned)
            {
                assigned = true;
                return string.IsNullOrWhiteSpace(existing)
                    ? ToShortString(Guid.NewGuid())
                    : existing;
            }

            public string ToShortString(Guid guid)
            {
                var base64Guid = Convert.ToBase64String(guid.ToByteArray());

                // Replace URL unfriendly characters with better ones
                base64Guid = base64Guid.Replace('+', '-').Replace('/', '_');

                // Remove the trailing ==
                return base64Guid.Substring(0, base64Guid.Length - 2);
            }
        }
    }

    public static class IdHelper
    {
        public static string GenerateId()
        {
            var base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            // Replace URL unfriendly characters with better ones
            base64Guid = base64Guid.Replace('+', '-').Replace('/', '_');

            // Remove the trailing ==
            return base64Guid.Substring(0, base64Guid.Length - 2);
        }
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
