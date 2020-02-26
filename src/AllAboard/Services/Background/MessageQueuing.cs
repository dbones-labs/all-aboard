namespace AllAboard.Services.Background
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class MessageQueuing
    {
        private readonly IScheduler _scheduler;
        private readonly ILogger<MessageQueuing> _logger;

        public MessageQueuing(IScheduler scheduler, ILogger<MessageQueuing> logger)
        {
            _scheduler = scheduler;
            _logger = logger;
        }

        public async Task ProcessMessages()
        {
            _logger.LogDebug("Try and publish new messages");
            _scheduler.Trigger();
        }
    }
}