namespace AllAboard.Configuration
{
    using Microsoft.Extensions.DependencyInjection;

    public interface IFactory
    {
        void RegisterServices(IServiceCollection services);
    }
}