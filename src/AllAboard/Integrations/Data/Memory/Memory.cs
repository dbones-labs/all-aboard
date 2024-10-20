namespace AllAboard.Integrations.Data.Memory
{
    using Configuration;
    using Data;
    using Microsoft.Extensions.DependencyInjection;

    public class Memory : IFactory
    {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<MemoryStore>();
            services.AddScoped<ISession, MemorySession>();
        }
    }
}