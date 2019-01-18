using AutoMapper;
using CheckerApi.Context;
using CheckerApi.Services;
using CheckerApi.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace CheckerApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper();
            services.AddMemoryCache();

            services.AddDbContext<ApiContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("Connection")));

            services.AddTransient<ISyncService, SyncService>();
            services.AddTransient<INotificationManager, NotificationManager>();
            services.AddTransient<IConditionComplier, ConditionComplier>();
            services.AddTransient<IRestClient, RestClient>();
            services.AddTransient<IAuditManager, AuditManager>();
            services.AddTransient<ICompressService, CompressService>();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHttpsRedirection();
            app.UseHsts();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvcWithDefaultRoute();
        }
    }
}
