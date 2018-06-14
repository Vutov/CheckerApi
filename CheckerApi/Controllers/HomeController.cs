using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CheckerApi.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Controllers
{
    [Route("")]
    public class HomeController : BaseController
    {
        public HomeController(IServiceProvider serviceProvider): base(serviceProvider)
        {
        }

        [HttpGet]
        [Route("")]
        public IActionResult Status()
        {
            var settings = typeof(ApiConfiguration)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToList();
            settings.RemoveAll(p => p.Name.ToLower() == "id");

            var conditions = Context.ConditionSettingsReadOnly
                .OrderBy(o => o.ConditionID)
                .Select(c => $"{c.ConditionName} ({c.Enabled})")
                .ToList();

            return Ok(new
            {
                Status = "Running",
                FoundOrders = Context.DataReadOnly.Count(),
                AuditCount = Context.OrdersAuditsReadOnly.Count(),
                Config = settings.Select(s => $"{s.Name} ({s.GetValue(Context.ConfigurationReadOnly)})"),
                Conditions = conditions
            });
        }

        [HttpGet]
        [Route("version")]
        public IActionResult GetVersion()
        {
            var env = ServiceProvider.GetService<IHostingEnvironment>();
            var build = System.IO.File.ReadAllText("./version.txt");
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            return Ok(new
            {
                BuildDate = build,
                Environment = env.EnvironmentName,
                Name = env.ApplicationName,
                Version = fvi.FileVersion
            });
        }
    }
}
