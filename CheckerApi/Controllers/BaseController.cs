using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CheckerApi.Context;
using CheckerApi.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Controllers
{
    public class BaseController: Controller
    {
        public BaseController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            var config = ServiceProvider.GetService<IConfiguration>();
            Password = config.GetValue<string>("Api:Password");
            Context = serviceProvider.GetService<ApiContext>();
        }

        public IServiceProvider ServiceProvider { get; }
        public ApiContext Context { get; }
        public string Password { get; }

        protected (ApiConfiguration config, List<PropertyInfo> configProps) GetConfig()
        {
            var config = Context.Configurations.OrderBy(o => o.ID).First();
            var type = config.GetType();
            var settings = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToList();
            settings.RemoveAll(p => p.Name.ToLower() == "id");

            return (config, settings);
        }
    }
}
