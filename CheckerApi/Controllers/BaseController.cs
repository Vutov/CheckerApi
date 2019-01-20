using System;
using CheckerApi.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Controllers
{
    public class BaseController: Controller
    {
        public BaseController(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Context = serviceProvider.GetService<ApiContext>();
        }

        public IServiceProvider ServiceProvider { get; }
        public ApiContext Context { get; }
    }
}
