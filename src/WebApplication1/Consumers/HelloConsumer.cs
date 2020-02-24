namespace WebApplication1.Consumers
{
    using System.Threading.Tasks;
    using MassTransit;

    public class HelloConsumer : IConsumer<Hello>
    {
        public Task Consume(ConsumeContext<Hello> context)
        {
            return Task.CompletedTask;
        }
    }
}