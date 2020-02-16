namespace AllAboard.Bus.MassTransit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using global::MassTransit;
    using GreenPipes;
    using Integrations.Bus;
    using Services;

    public class BusAdapter : IBusAdapter
    {
        private readonly IPublishEndpoint _bus;
    

        public BusAdapter(IPublishEndpoint bus)
        {
            _bus = bus;
        }

        public Task Publish(object message, CancellationToken token = default(CancellationToken))
        {
            return _bus.Publish(message, token);
        }

        public Task Publish(MessageEntry message, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
                //_bus.Publish()
        }
    }

    public class OpenTracingPipeSpecification : IPipeSpecification<ConsumeContext>, IPipeSpecification<PublishContext>
    {
        public IEnumerable<ValidationResult> Validate()
        {
            return Enumerable.Empty<ValidationResult>();
        }

        public void Apply(IPipeBuilder<ConsumeContext> builder)
        {
            builder.AddFilter(new OpenTracingConsumeFilter<ConsumeContext>());
        }

        public void Apply(IPipeBuilder<PublishContext> builder)
        { 
            //builder.AddFilter(new OpenTracingPublishFilter());
        }
    }


    public class OpenTracingConsumeFilter<T> : IFilter<T> where T : class, ConsumeContext
    {
        public Task Send(T context, IPipe<T> next)
        {
            var scope = context.GetPayload<IServiceProvider>();
            
            context.
            return next.Send(context);


        }

        public void Probe(ProbeContext context)
        {
        }
    }

    public static class OpenTracingMiddlewareConfiguratorExtensions
    {
        public static void UseAllAboard(this IBusFactoryConfigurator value)
        {
            value.ConfigurePublish(configurator => configurator.AddPipeSpecification(new OpenTracingPipeSpecification()));
            value.AddPipeSpecification(new OpenTracingPipeSpecification());
        }
    }

}

