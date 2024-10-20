namespace AllAboard.Bus.MassTransit
{
    using System;
    using System.Threading.Tasks;
    using global::MassTransit;
    using GreenPipes;
    using Integrations.Data;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using Services.Background;
    using Services.Consuming;

    public class ConsumeFilter<T> : IFilter<T> where T : class, ConsumeContext
    {
        public async Task Send(T context, IPipe<T> next)
        {
            var scope = context.GetPayload<IServiceProvider>();
            var idStrategy = new IdStrategy();

            //filter if we have already processed the message
            var messageFilter = scope.GetService<MessageFilter>();
            var id = idStrategy.ConvertFromProvider(context.MessageId);
            var hasProcessedMessage = await messageFilter.HasProcessedMessage(id);
            if (hasProcessedMessage)
            {
                return;
            }

            //grab some information about the current context
            var ctx = scope.GetService<ConsumingMessageContext>();
            ctx.Message = new MessageEntry()
            {
                CorrelationId = idStrategy.ConvertFromProvider(context.CorrelationId),
                SourceId = idStrategy.ConvertFromProvider(context.MessageId)
            };


            //process as normal
            await next.Send(context);

            //mark as processed and commit any changes.
            await messageFilter.MarkMessageAsProcessed();
            var session = scope.GetService<ISession>();
            await session.Commit();

            //note this is not as imoprtant, we will eventually process the messages
            var messageQueuing = scope.GetService<MessageQueuing>();
            await messageQueuing.ProcessMessages();
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}