namespace AllAboard.Services.Background
{
    using System.Threading;

    public interface IScheduler
    {
        void Trigger();

        void Start(CancellationToken cancellationToken = default);
    }
}