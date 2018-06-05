using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        [Route("data/{top?}")]
        public IActionResult Get(int top = 10)
        {
            return Ok(Context.Data.OrderByDescending(i => i.RecordDate).Take(top).ToList());
        }

        [HttpGet]
        [Route("")]
        public IActionResult Status()
        {
            var (config, configSettings) = GetConfig();
            var conditions = Context.ConditionSettings
                .OrderBy(o => o.ConditionID)
                .Select(c => $"{c.ConditionName} ({c.Enabled})")
                .ToList();

            return Ok(new
            {
                Status = "Running",
                FoundOrders = Context.Data.Count(),
                Config = configSettings.Select(s => $"{s.Name} ({s.GetValue(config)})"),
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
