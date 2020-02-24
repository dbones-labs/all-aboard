namespace AllAboard.Services.Background
{
    using System.Collections.Generic;
    using System.Threading;

    public interface IScheduler
    {
        void Enqueue(IEnumerable<MessageEntry> messageEntries);
        void Start(CancellationToken cancellationToken = default);
    }
}