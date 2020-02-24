namespace WebApplication1
{
    using System.Threading.Tasks;
    using AllAboard.Services.Background;
    using Marten;
    using Microsoft.AspNetCore.Http;

    public class MartenMiddleware
    {
        private readonly RequestDelegate _next;

        public MartenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // IMyScopedService is injected into Invoke
        public async Task Invoke(HttpContext httpContext, IDocumentSession session)
        {
            await _next(httpContext);
            await session.SaveChangesAsync();
        }
    }


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