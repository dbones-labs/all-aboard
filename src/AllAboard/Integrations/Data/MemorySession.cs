namespace AllAboard.Integrations.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Services;

    public class MemorySession : ISession
    {
        private readonly MemoryStore _store;
        private readonly List<Entity> _transactionStore = new List<Entity>();

        public MemorySession(MemoryStore store)
        {
            _store = store;
        }

        public void Add<T>(T entity) where T : Entity
        {
            _transactionStore.Add(entity);
        }

        public void Remove(MessageEntry publishedMessaged)
        {
            
        }

        public Task<bool> HasProcessedMessage(string messageId)
        {
            var result = _store.Store.Any(x => x.Id == messageId);
            return Task.FromResult(result);
        }

        public Task<IEnumerable<MessageEntry>> GetPublishedMessages()
        {
            var items =_store.Store.Where(x => x is MessageEntry);
            return Task.FromResult((IEnumerable<MessageEntry>)items.ToArray());
        }

        public Task Commit()
        {
            foreach (var entity in _transactionStore)
            {
                if (_store.Store.Any(x=> x == entity))
                {
                    continue;
                }

                _store.Store.Add(entity);
            }

            return Task.CompletedTask;
            
        }
    }
}