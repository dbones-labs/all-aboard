namespace WebApplication1
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