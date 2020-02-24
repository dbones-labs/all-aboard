namespace AllAboard.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Integrations.Bus;
    using Integrations.Data;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;


    /// <summary>
    /// the message filter manages if the client (subscribing service) has processed a message off the queue.
    /// </summary>
    /// <remarks>
    /// scoped to the request, as this holds state.
    /// </remarks>
    public class MessageFilter
    {
        private readonly ISession _session;
        private readonly ILogger<MessageFilter> _logger;
        private ProcessedMessage _processedMessage;
        private string _messageInfo;

        public MessageFilter(ISession session, ILogger<MessageFilter> logger)
        {
            _session = session;
            _logger = logger;
        }

        /// <summary>
        /// run this at the start of processing a message to see if we have already processed it.
        /// </summary>
        /// <param name="messageId">the message Id</param>
        /// <param name="topic">the topic of the message</param>
        /// <returns>true if the message as already been processed</returns>
        public virtual async Task<bool> HasProcessedMessage(string messageId, Type topic = null)
        {
            _messageInfo = $"{topic} - {messageId}";
            var alreadyProcessed = await _session.HasProcessedMessage(messageId);
            if (alreadyProcessed)
            {
                _logger.LogInformation($"Already processed {_messageInfo}");
                return true;
            }

            var markedProcessedMessage = new ProcessedMessage()
            {
                Id = messageId,
                TopicType = topic
            };

            _processedMessage = markedProcessedMessage;
            _logger.LogDebug($"New message {_messageInfo}");

            return false;
        }

        /// <summary>
        /// we need to mark the message as processed
        /// </summary>
        public virtual async Task MarkMessageAsProcessed()
        {
            var alreadyProcessed = await _session.HasProcessedMessage(_processedMessage.Id);
            if (alreadyProcessed) throw new Exception($"Message has been processed {_messageInfo}");

            //we have delayed the add to this point "post-processing", to allow us to run the query above
            _session.Add(_processedMessage);
        }
    }

    public class BackgroundMessageQueuing
    {
        private readonly ISession _session;
        private readonly IScheduler _scheduler;
        private readonly ILogger<BackgroundMessageQueuing> _logger;

        public BackgroundMessageQueuing(ISession session, IScheduler scheduler, ILogger<BackgroundMessageQueuing> logger)
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

    public interface IScheduler
    {
        void Enqueue(IEnumerable<MessageEntry> messageEntries);
        void Start(CancellationToken cancellationToken = default);
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
        private readonly ILogger<BackgroundScheduler> _logger;
        private readonly ManualResetEventSlim _manualEvent = new ManualResetEventSlim();
        private volatile bool _isRunning = true;
        private readonly object _lock = new object();
        private readonly Queue<MessageEntry> _toProcess = new Queue<MessageEntry>();
        private Task _backgroundWorker;

        public BackgroundScheduler(IServiceScope scope, ILogger<BackgroundScheduler> logger)
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
                    MessageEntry entry;
                    while ((entry = _toProcess.Dequeue()) != null)
                    {
                        messages.Add(entry);
                    }
                }

                _logger.LogInformation($"publishing {messages.Count} messages");
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
            //note each message is processed individually incase of failure
            //we do not want to loose progress, and minimise duplicates being published
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
            _entry.Message = message;
            await _busAdapter.Publish(message);
            await _session.Commit();
            _logger.LogDebug($"published {message.Id}");
        }
    }

    public class PublishMessageContext
    {
        public MessageEntry Message { get; set; }
    }

    public class ConsumingMessageContext
    {
        public MessageEntry Message { get; set; }
    }

}