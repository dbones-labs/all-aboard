namespace WebApplication1.Infrastructure.AllAboard
{
    using System.Threading.Tasks;
    using global::AllAboard.Services.Background;
    using global::AllAboard.Services.Consuming;
    using Microsoft.AspNetCore.Http;

    public class AllAboardMiddleware
    {
        private readonly RequestDelegate _next;

        public AllAboardMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task Invoke(HttpContext httpContext, MessageQueuing queuing, ConsumingMessageContext consumingMessageContext)
        {
            await _next(httpContext);
            await queuing.ProcessMessages();
        }
    }
}