namespace WebApplication1.Infrastructure.AllAboard
{
    using System.Threading.Tasks;
    using global::AllAboard.Services.Background;
    using Microsoft.AspNetCore.Http;

    public class AllAboardMiddleware
    {
        private readonly RequestDelegate _next;

        public AllAboardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // IMyScopedService is injected into Invoke
        public async Task Invoke(HttpContext httpContext, MessageQueuing queuing)
        {
            await _next(httpContext);
            await queuing.ProcessMessages();
        }
    }
}