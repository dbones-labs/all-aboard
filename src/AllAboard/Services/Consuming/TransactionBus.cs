namespace AllAboard.Services.Consuming
{
    using Integrations.Bus;
    using Integrations.Data;

    public class TransactionBus : IBus
    {
        private readonly ISession _session;
        private readonly IIdStrategy _idStrategy;
        private readonly ConsumingMessageContext _consumingContext;

        public TransactionBus(
            ISession session,
            IIdStrategy idStrategy,
            ConsumingMessageContext consumingContext)
        {
            _session = session;
            _idStrategy = idStrategy;
            _consumingContext = consumingContext;
        }

        public void Publish<T>(T message)
        {
            var initiatorMessage = _consumingContext.Message;

            var id = _idStrategy.ConvertFromProvider(_idStrategy.NewId());
            var correlationId = initiatorMessage == null 
                ? _idStrategy.ConvertFromProvider(_idStrategy.NewId()) 
                : initiatorMessage.CorrelationId;

            var entry = new MessageEntry
            {
                Body = message,

                Id = id,
                SourceId = initiatorMessage?.Id,
                CorrelationId = correlationId,
                TopicType = typeof(T)
            };


            _session.Add(entry);
        }
    }
}