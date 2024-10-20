namespace AllAboard.Services.Consuming
{
    using System;
    using System.Threading.Tasks;
    using Integrations.Data;
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
                _logger.LogInformation($"Already processed message, Id: {_messageInfo}");
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
        /// note if the message has been processed during this threads exexution then we will throw an exception
        /// in order to cancel its database transaction. this will then cause the message to be re-processed
        /// and then it will be picked up by the <see cref="HasProcessedMessage"/> method and skipped.
        /// </summary>
        public virtual async Task MarkMessageAsProcessed()
        {
            //try to confirm if the message has been processed by another thread.
            var alreadyProcessed = await _session.HasProcessedMessage(_processedMessage.Id);
            if (alreadyProcessed) throw new Exception($"Message has been processed {_messageInfo}");

            //we have delayed the add to this point "post-processing", to allow us to run the query above
            //Note, we also hope that the commit will fail if the message has been process by another thread
            _session.Add(_processedMessage);
        }
    }
}