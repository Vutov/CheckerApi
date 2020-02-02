using AutoMapper;
using CheckerApi.Context;
using CheckerApi.Services;
using CheckerApi.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Checker API", Version = "v1" });
            });

            services.AddTransient<ISyncService, SyncService>();
            services.AddTransient<INotificationManager, NotificationManager>();
            services.AddTransient<IConditionCompiler, ConditionCompiler>();
            services.AddTransient<IRestClient, RestClient>();
            services.AddTransient<IAuditManager, AuditManager>();
            services.AddTransient<ICompressService, CompressService>();
            services.AddTransient<IPoolPullService, PoolPullService>();
            services.AddTransient<IDataExtractorService, DataExtractorService>();
            services.AddTransient<IForkWatchService, ForkWatchService>();

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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Checker API V1");
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
