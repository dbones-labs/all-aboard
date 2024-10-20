namespace AllAboard.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Background;
    using Microsoft.Extensions.Hosting;

    public class BusHostedService : IHostedService
    {
        private readonly IScheduler _scheduler;

        public BusHostedService(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        { 
            _scheduler.Start(cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}