namespace AllAboard.Bus.MassTransit
{
    using System.Threading;
    using System.Threading.Tasks;
    using global::MassTransit;
    using Integrations.Bus;
    using Services;

    public class BusAdapter : IBusAdapter
    {
        private readonly IPublishEndpoint _bus;
        private readonly IIdStrategy _idStrategy;


        public BusAdapter(IPublishEndpoint bus, IIdStrategy idStrategy)
        {
            _bus = bus;
            _idStrategy = idStrategy;
        }

        public async Task Publish(MessageEntry message, CancellationToken token = default)
        {
            await _bus.Publish(message, new ApplyHeadersPipe(message, _idStrategy), token);
        }
    }
}