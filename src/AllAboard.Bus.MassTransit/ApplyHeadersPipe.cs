namespace AllAboard.Bus.MassTransit
{
    using System;
    using System.Threading.Tasks;
    using global::MassTransit;
    using GreenPipes;
    using Integrations.Bus;
    using Services;

    public class ApplyHeadersPipe : IPipe<PublishContext<MessageEntry>>
    {
        private readonly MessageEntry _entry;
        private readonly IIdStrategy _idStrategy;

        public ApplyHeadersPipe(MessageEntry entry, IIdStrategy idStrategy)
        {
            _entry = entry;
            _idStrategy = idStrategy;
        }

        public Task Send(PublishContext<MessageEntry> context)
        {
            context.MessageId = (Guid?)_idStrategy.ConvertToProvider(_entry.Id);
            context.CorrelationId = (Guid?) _idStrategy.ConvertToProvider(_entry.CorrelationId);
            context.InitiatorId = (Guid?) _idStrategy.ConvertToProvider(_entry.SourceId);
           

            foreach (var header in _entry.Headers)
            {
                context.Headers.Set(header.Key, header.Value);
            }

            

            return Task.CompletedTask;
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}