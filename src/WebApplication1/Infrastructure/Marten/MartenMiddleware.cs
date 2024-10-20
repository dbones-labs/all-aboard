namespace WebApplication1.Infrastructure.Marten
{
    using System.Threading.Tasks;
    using global::Marten;
    using Microsoft.AspNetCore.Http;

    public class MartenMiddleware
    {
        private readonly RequestDelegate _next;

        public MartenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IDocumentSession session)
        {
            await _next(httpContext);
            await session.SaveChangesAsync();
        }
    }
}