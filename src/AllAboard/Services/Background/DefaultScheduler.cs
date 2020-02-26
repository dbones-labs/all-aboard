namespace AllAboard.Services.Background
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Integrations.Data;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DefaultScheduler : IScheduler, IDisposable
    {
        private readonly IServiceProvider _scope;
        private readonly ILogger<DefaultScheduler> _logger;
        private readonly ManualResetEventSlim _manualEvent = new ManualResetEventSlim();
        private volatile bool _isRunning = true;
        private readonly object _lock = new object();
        private Task _backgroundWorker;

        public DefaultScheduler(IServiceProvider scope, ILogger<DefaultScheduler> logger)
        {
            _scope = scope;
            _logger = logger;
        }

        public void Trigger()
        {
            _manualEvent.Set();
        }

        private async Task<IEnumerable<MessageEntry>> GetMessagesToProcess()
        {
            using (var scope = _scope.CreateScope())
            {
                var session = scope.ServiceProvider.GetService<ISession>();
                return await session.GetPublishedMessages();
            }
        }

        public void Start(CancellationToken cancellationToken = default)
        {
            _backgroundWorker = new Task(() => BackgroundWorker(cancellationToken));
            _backgroundWorker.Start();
        }

        public void BackgroundWorker(CancellationToken cancellationToken = default)
        {
            bool IsRunning() => _isRunning && !cancellationToken.IsCancellationRequested;

            while (IsRunning())
            {
                _manualEvent.Wait(cancellationToken);
                if (!IsRunning()) return;
                _manualEvent.Reset();

                _logger.LogDebug("get enqueued messages");
                var tasks = new List<Task>();
                var messages = GetMessagesToProcess().Result.ToList();

                _logger.LogInformation($"publishing {messages.Count} messages");
                foreach (var messageId in messages.Select(x=> x.Id).Distinct())
                {
                    var t = ProcessMessageEntry(messageId);
                    //t.Start();
                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
                _logger.LogInformation($"published {messages.Count} messages");
            }
        }

        private async Task ProcessMessageEntry(string messageId)
        {
            //note each message is processed individually in-case of failure
            //we do not want to loose progress, and minimise duplicates being published
            using (var childScope = _scope.CreateScope())
            {
                try
                {
                    var processor = childScope.ServiceProvider.GetService<MessageProcessor>();
                    await processor.Process(messageId);
                }
                catch (Exception e)
                {
                    //we do not want to crash the main thread, so we will log out, and try again.
                    _logger.LogError(e, "failed to process message");
                }

            }

        }

        public void Dispose()
        {
            _isRunning = false;
            _manualEvent?.Dispose();
            Task.WaitAll(_backgroundWorker);
        }
    }
}