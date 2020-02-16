namespace AllAboard.Services
{
    using System;
    using System.Collections.Generic;
    using Integrations;
    using Integrations.Data;

    public class TransactionBus : IBus
    {
        private readonly ISession _session;
        private readonly IEnumerable<IContextExtractor> _extractors;
        private readonly ProcessingContext _context;

        public TransactionBus(ISession session, IEnumerable<IContextExtractor> extractors, ProcessingContext context)
        {
            _session = session;
            _extractors = extractors;
            _context = context;
        }

        public void Publish<T>(T message)
        {
            var entry = new MessageEntry
            {
                Body = message, 
                Id = Guid.NewGuid().ToString("D")
            };

            foreach (var extractor in _extractors)
            {
                extractor.Populate(entry, _context);
            }

            _session.Add(entry);
        }
    }
}