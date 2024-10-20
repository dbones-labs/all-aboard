namespace WebApplication1.Consumers
{
    using System.Threading.Tasks;
    using MassTransit;
    using Microsoft.Extensions.Logging;

    public class HelloConsumer : IConsumer<Hello>
    {
        private readonly ILogger<HelloConsumer> _logger;

        public HelloConsumer(ILogger<HelloConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<Hello> context)
        {
            _logger.LogInformation("hello");
            return Task.CompletedTask;
        }
    }
}