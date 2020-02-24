namespace AllAboard.Data.Marten
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Configuration;
    using global::Marten;
    using Integrations.Data;
    using Microsoft.Extensions.DependencyInjection;
    using Services;

    public class Marten : IDataStoreProvider {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ISession, ISession>();
        }
    }


    public class Session : ISession
    {
        private readonly IDocumentSession _session;

        public Session(IDocumentSession session)
        {
            _session = session;
        }

        public void Add<T>(T entity) where T : Entity
        {
            _session.Insert(entity);
        }

        public void Remove(MessageEntry publishedMessaged)
        {
            _session.Delete(publishedMessaged);
        }

        public async Task<bool> HasProcessedMessage(string messageId)
        {
            bool processed = await _session
                .Query<ProcessedMessage>()
                .AnyAsync(x => x.Id == messageId);
            return processed;
        }

        public Task<IEnumerable<MessageEntry>> GetPublishedMessages()
        {
            IEnumerable<MessageEntry> items = _session
                .Query<MessageEntry>()
                .OrderByDescending(x => x.ProcessedAt)
                .ToList();
            return Task.FromResult(items);
        }

        public Task<MessageEntry> GetPublishedMessage(string id)
        {
            var message = _session.Query<MessageEntry>().SingleOrDefault(x => x.Id == id);
            return Task.FromResult(message);
        }

        public Task Commit()
        {
            return _session.SaveChangesAsync();
        }
    }
}
