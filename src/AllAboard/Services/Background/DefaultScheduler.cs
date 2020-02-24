namespace AllAboard.Services.Background
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DefaultScheduler : IScheduler, IDisposable
    {
        private readonly IServiceProvider _scope;
        private readonly ILogger<DefaultScheduler> _logger;
        private readonly ManualResetEventSlim _manualEvent = new ManualResetEventSlim();
        private volatile bool _isRunning = true;
        private readonly object _lock = new object();
        private readonly Queue<MessageEntry> _toProcess = new Queue<MessageEntry>();
        private Task _backgroundWorker;

        public DefaultScheduler(IServiceProvider scope, ILogger<DefaultScheduler> logger)
        {
            _scope = scope;
            _logger = logger;
        }

        public void Enqueue(IEnumerable<MessageEntry> messageEntries)
        {
            lock (_lock)
            {
                foreach (var messageEntry in messageEntries)
                {
                    _logger.LogDebug($"enqueuing {messageEntry.Id} for publishing");
                    _toProcess.Enqueue(messageEntry);
                }
            }
            _manualEvent.Set();
        }

        public void Start(CancellationToken cancellationToken = default(CancellationToken))
        {
            _backgroundWorker = new Task(() => BackgroundWorker(cancellationToken));
            _backgroundWorker.Start();
        }

        public void BackgroundWorker(CancellationToken cancellationToken = default(CancellationToken))
        {
            bool IsRunning() => _isRunning && !cancellationToken.IsCancellationRequested;

            while (IsRunning())
            {
                _manualEvent.Wait(cancellationToken);
                if (!IsRunning()) return;
                _manualEvent.Reset();

                var tasks = new List<Task>();
                var messages = new List<MessageEntry>();

                _logger.LogDebug("get enqueued messages");
                lock (_lock)
                {
                    while (_toProcess.Any())
                    {
                        var entry = _toProcess.Dequeue();
                        messages.Add(entry);
                    }
                }

                _logger.LogInformation($"publishing {messages.Count} messages");
                foreach (var messageEntry in messages)
                {
                    var t = ProcessMessageEntry(messageEntry);
                    //t.Start();
                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        private async Task ProcessMessageEntry(MessageEntry messageEntry)
        {
            //note each message is processed individually incase of failure
            //we do not want to loose progress, and minimise duplicates being published
            using (var childScope = _scope.CreateScope())
            {
                var processor = childScope.ServiceProvider.GetService<MessageProcessor>();
                await processor.Process(messageEntry.Id);
            }
        }

        public void Dispose()
        {
            _isRunning = false;
            _manualEvent?.Dispose();
            _backgroundWorker?.Dispose();
        }
    }
}