namespace AllAboard.Integrations.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Services;

    public interface ISession
    {
        void Add<T>(T entity) where T : Entity;
        void Remove(MessageEntry publishedMessaged);
        Task<bool> HasProcessedMessage(string messageId);
        Task<IEnumerable<MessageEntry>> GetPublishedMessages();
        Task<MessageEntry> GetPublishedMessage(string id);

        Task Commit();
    }
}