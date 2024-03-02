using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace OwlApi.Helpers
{
    public static class MapSubdomainExtensions
    {
        public static IApplicationBuilder MapSubdomain(this IApplicationBuilder app,
            string subdomain, Action<IApplicationBuilder> configuration)
        {
            return app.MapWhen(GetSubdomainPredicate(subdomain), configuration);
        }

        private static Func<HttpContext, bool> GetSubdomainPredicate(string subdomain)
        {
            return (context) =>
            {
                var split = context.Request.Host.Host.Split('.');
                if (split.Length < 4) return subdomain == "";
                return subdomain == split[0];
            };
        }
    }
}
