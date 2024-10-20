namespace AllAboard.Services.Background
{
    using System.Threading.Tasks;
    using Integrations.Bus;
    using Integrations.Data;
    using Microsoft.Extensions.Logging;

    public class MessageProcessor
    {
        private readonly ISession _session;
        private readonly IBusAdapter _busAdapter;
        private readonly PublishMessageContext _entry;
        private readonly ILogger<MessageProcessor> _logger;

        public MessageProcessor(
            ISession session, 
            IBusAdapter busAdapter,
            PublishMessageContext entry,
            ILogger<MessageProcessor> logger)
        {
            _session = session;
            _busAdapter = busAdapter;
            _entry = entry;
            _logger = logger;
        }

        public async Task Process(string messageId)
        {
            var message = await _session.GetPublishedMessage(messageId);

            if (message == null)
            {
                _logger.LogInformation($"might have already processed message id: {messageId}");
                return;
            }

            _entry.Message = message;

            await _busAdapter.Publish(message);
            _session.Remove(message);
            
            await _session.Commit();
            _logger.LogInformation($"published {message.TopicType.FullName} {message.Id}");
        }
    }
}