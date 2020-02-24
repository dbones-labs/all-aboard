namespace AllAboard.Bus.MassTransit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Configuration;
    using global::MassTransit;
    using GreenPipes;
    using Integrations.Bus;
    using Integrations.Data;
    using Microsoft.Extensions.DependencyInjection;
    using Services;


    public class Masstransit : IMessagingProvider {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IBusAdapter, BusAdapter>();
        }
    }

    public class BusAdapter : IBusAdapter
    {
        private readonly IPublishEndpoint _bus;
    

        public BusAdapter(IPublishEndpoint bus)
        {
            _bus = bus;
        }

        public async Task Publish(MessageEntry message, CancellationToken token = default)
        {
            await _bus.Publish(message.Body, token);
        }
    }

    public class AllAboardPipeSpecification : IPipeSpecification<ConsumeContext>, IPipeSpecification<PublishContext>
    {
        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new AllAboardConsumeFilter<ConsumeContext>());
        }

        public void Apply(IPipeBuilder<PublishContext> builder)
        { 
            builder.AddFilter(new AllAboardPublishFilter<PublishContext>());
        }
    }


    public class AllAboardConsumeFilter<T> : IFilter<T> where T : class, ConsumeContext
    {
        public async Task Send(T context, IPipe<T> next)
        {
            var scope = context.GetPayload<IServiceProvider>();

            //filter if we have already processed the message
            var messageFilter = scope.GetService<MessageFilter>();
            var hasProcessedMessage = await messageFilter.HasProcessedMessage(context.MessageId.ToString());
            if (hasProcessedMessage)
            {
                return;
            }

            //grab some information about the current context
            var message = scope.GetService<MessageEntry>();
            message.CorrelationId = context.CorrelationId.ToString();
            message.SourceId = context.MessageId.ToString();


            //process as normal
            await next.Send(context);

            //mark as processed and commit any changes.
            await messageFilter.MarkMessageAsProcessed();
            var session = scope.GetService<ISession>();
            await session.Commit();

            //note this is not as imoprtant, we will eventually process the messages
            var messageQueuing = scope.GetService<BackgroundMessageQueuing>();
            await messageQueuing.ProcessMessages();
        }

        public void Probe(ProbeContext context)
        {
        }
    }

    public class AllAboardPublishFilter<T> : IFilter<T> where T : class, PublishContext
    {
        public async Task Send(T context, IPipe<T> next)
        {
            var scope = context.GetPayload<IServiceProvider>();
            await next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }
    }



    public static class AllAboardMiddlewareConfiguratorExtensions
    {
        public static void UseAllAboard(this IBusFactoryConfigurator value)
        {
            value.ConfigurePublish(configurator => configurator.AddPipeSpecification(new AllAboardPipeSpecification()));
            value.AddPipeSpecification(new AllAboardPipeSpecification());
        }
    }

}

