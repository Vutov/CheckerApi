using CheckerApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;

namespace CheckerApi.Extensions
{
    public static class HttpsRedirectExtensions
    {
        public static void SetupHttpsRedirect(this IApplicationBuilder app, IConfiguration configuration, IHostingEnvironment env)
        {
            var httpsSection = configuration.GetSection("HttpServer:Endpoints:Https");

            if (httpsSection.Exists())
            {
                var httpsEndpoint = new EndpointConfiguration();
                httpsSection.Bind(httpsEndpoint);
                var httpsPort = httpsEndpoint.Port;
                var statusCode = env.IsDevelopment() ? StatusCodes.Status302Found : StatusCodes.Status301MovedPermanently;
                app.UseRewriter(new RewriteOptions().AddRedirectToHttps(statusCode, httpsPort));
            }
        }
    }
}
