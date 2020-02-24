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
    

        public BusAdapter(IPublishEndpoint bus)
        {
            _bus = bus;
        }

        public async Task Publish(MessageEntry message, CancellationToken token = default)
        {
            await _bus.Publish(message.Body, token);
        }
    }
}