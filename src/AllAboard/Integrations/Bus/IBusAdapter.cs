namespace AllAboard.Integrations.Bus
{
    using System.Threading;
    using System.Threading.Tasks;
    using Services;

    /// <summary>
    /// 
    /// </summary>
    public interface IBusAdapter
    {
        Task Publish(MessageEntry message, CancellationToken token = default(CancellationToken));
    }
}