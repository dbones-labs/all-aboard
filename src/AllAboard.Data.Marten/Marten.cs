namespace AllAboard.Data.Marten
{
    using Configuration;
    using Integrations.Data;
    using Microsoft.Extensions.DependencyInjection;

    public class Marten : IDataStoreProvider {
        public void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<ISession, Session>();
        }
    }
}
