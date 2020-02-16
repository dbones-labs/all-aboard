namespace AllAboard.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Integrations.Bus;
    using Integrations.Data;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;


    /// <summary>
    /// the message filter manages if the client (subscribing service) has processed a message off the queue.
    /// </summary>
    public class MessageFilter
    {
        private readonly ISession _session;
        private readonly ProcessingContext _context;

        public MessageFilter(ISession session, ProcessingContext context)
        {
            _session = session;
            _context = context;
        }

        /// <summary>
        /// run this at the start of processing a message to see if we have already processed it.
        /// </summary>
        /// <param name="messageId">the message Id</param>
        /// <param name="topic">the topic of the message</param>
        /// <returns>true if the message as already been processed</returns>
        public virtual async Task<bool> HasProcessedMessage(string messageId, Type topic = null)
        {
            var alreadyProcessed = await _session.HasProcessedMessage(messageId);
            if (alreadyProcessed) return true;

            var markedProcessedMessage = new ProcessedMessage()
            {
                Id = messageId,
                TopicType = topic
            };

            _context.ProcessedMessage = markedProcessedMessage;
            
            return false;
        }

        /// <summary>
        /// we need to mark the message as processed
        /// </summary>
        public virtual async Task MarkMessageAsProcessed()
        {
            var alreadyProcessed = await _session.HasProcessedMessage(_context.ProcessedMessage.Id);
            if (alreadyProcessed) throw new Exception("message has been processed");

            //we have delayed the add to this point "post-processing", to allow us to run the query above
            _session.Add(_context.ProcessedMessage);
        }
    }

    public class BackgroundMessageQueuing
    {
        private readonly ISession _session;
        private readonly IScheduler _scheduler;

        public BackgroundMessageQueuing(ISession session, IScheduler scheduler)
        {
            _session = session;
            _scheduler = scheduler;
        }

        public async Task ProcessMessages()
        {
            var messages = await _session.GetPublishedMessages();
            _scheduler.Enqueue(messages);
        }
    }

    public interface IScheduler
    {
        void Enqueue(IEnumerable<MessageEntry> messageEntries);
        void Start(CancellationToken cancellationToken = default(CancellationToken));
    }

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

    public class BackgroundScheduler : IScheduler, IDisposable
    {
        private readonly IServiceScope _scope;
        private readonly ManualResetEventSlim _manualEvent = new ManualResetEventSlim();
        private volatile bool _isRunning = true;
        private readonly object _lock = new object();
        private readonly Queue<MessageEntry> _toProcess = new Queue<MessageEntry>();
        private Task _backgroundWorker;

        public BackgroundScheduler(IServiceScope scope)
        {
            _scope = scope;
        }

        public void Enqueue(IEnumerable<MessageEntry> messageEntries)
        {
            lock (_lock)
            {
                foreach (var messageEntry in messageEntries)
                {
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

                lock (_lock)
                {
                    MessageEntry entry;
                    while ((entry = _toProcess.Dequeue()) != null)
                    {
                        messages.Add(entry);
                    }
                }

                foreach (var messageEntry in messages)
                {
                    var t = ProcessMessageEntry(messageEntry);
                    t.Start();
                    tasks.Add(t);
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        private async Task ProcessMessageEntry(MessageEntry messageEntry)
        {
            using (var childScope = _scope.ServiceProvider.CreateScope())
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


    public class MessageProcessor
    {
        private readonly ISession _session;
        private readonly IBusAdapter _busAdapter;

        public MessageProcessor(ISession session, IBusAdapter busAdapter)
        {
            _session = session;
            _busAdapter = busAdapter;
        }

        public async Task Process(string messageId)
        {
            var message = await _session.GetPublishedMessage(messageId);
            await _busAdapter.Publish(message);
        }
    }

}