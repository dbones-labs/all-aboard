namespace AllAboard.Bus.MassTransit
{
    using System;
    using System.Threading.Tasks;
    using global::MassTransit;
    using GreenPipes;
    using Microsoft.Extensions.DependencyInjection;

    public class AllAboardPublishFilter<T> : IFilter<T> where T : class, PublishContext
    {
        public async Task Send(T context, IPipe<T> next)
        {
            var scope = context.GetPayload<IServiceProvider>();
            await next.Send(context);
        }

        public void Probe(ProbeContext context)
        {
        }
    }
}