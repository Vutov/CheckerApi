using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Filters
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthenticateFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var config = context.HttpContext.RequestServices.GetService<IConfiguration>();
            var apiPassword = config.GetValue<string>("Api:Password");
            if (string.IsNullOrEmpty(apiPassword))
            {
                return;
            }

            var found = context.ActionArguments.TryGetValue("password", out var password);
            if (found == false || !string.Equals(apiPassword, password.ToString(), StringComparison.Ordinal))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}