namespace AllAboard.Services.Background
{
    using System.Threading.Tasks;
    using Integrations.Data;
    using Microsoft.Extensions.Logging;

    public class MessageQueuing
    {
        private readonly ISession _session;
        private readonly IScheduler _scheduler;
        private readonly ILogger<MessageQueuing> _logger;

        public MessageQueuing(ISession session, IScheduler scheduler, ILogger<MessageQueuing> logger)
        {
            _session = session;
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task ProcessMessages()
        {
            _logger.LogDebug("Try and publish new messages");
            var messages = await _session.GetPublishedMessages();
            _scheduler.Enqueue(messages);
        }
    }
}