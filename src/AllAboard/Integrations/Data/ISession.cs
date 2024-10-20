namespace AllAboard.Integrations.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Services;

    public interface ISession
    {
        /// <summary>
        /// add the entity to the backing store
        /// note this should be the processed messages for the filter
        /// and for messages which have been published in process and are queued for publishing to the bus at a later time
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        void Add<T>(T entity) where T : Entity;

        /// <summary>
        /// remove a message which has now been published to the bus
        /// </summary>
        /// <param name="publishedMessaged"></param>
        void Remove(MessageEntry publishedMessaged);

        /// <summary>
        /// check if a message has already be consumed
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns>true if the message has been processed</returns>
        Task<bool> HasProcessedMessage(string messageId);

        /// <summary>
        /// get the messages which need to be published to the bus
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<MessageEntry>> GetPublishedMessages();

        /// <summary>
        /// get a particular message from its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<MessageEntry> GetPublishedMessage(string id);

        /// <summary>
        /// commit all in memory changes to the backing store
        /// </summary>
        /// <returns></returns>
        Task Commit();
    }
}