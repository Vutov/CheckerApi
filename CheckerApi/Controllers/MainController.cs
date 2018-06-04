using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CheckerApi.Context;
using CheckerApi.Data.Entities;
using CheckerApi.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Controllers
{
    [Route("")]
    public class MainController : Controller
    {
        private readonly string _password;
        private readonly ApiContext _context;
        private readonly IServiceProvider _serviceProvider;

        public MainController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var config = _serviceProvider.GetService<IConfiguration>();
            _password = config.GetValue<string>("Api:Password");
            _context = serviceProvider.GetService<ApiContext>();
        }

        [HttpGet]
        [Route("data/{top?}")]
        public IActionResult Get(int top = 10)
        {
            return Ok(_context.Data.OrderByDescending(i => i.RecordDate).Take(top).ToList());
        }

        [HttpGet]
        [Route("")]
        public IActionResult Status()
        {
            var (_, settings) = this.GetConfig();
            var conditions = _context.ConditionSettings
                .OrderBy(o => o.ID)
                .Select(c => new { Name = c.ConditionName, c.Enabled })
                .ToList();

            return Ok(new
            {
                Status = "Running",
                FoundOrders = _context.Data.Count(),
                Settings = settings.Select(s => s.Name),
                Conditions = conditions
            });
        }

        [HttpGet]
        [Route("{setting}/{rate}/{password?}")]
        public IActionResult SetSetting(string setting, double rate, string password = "")
        {
            if (_password != password || string.IsNullOrEmpty(setting))
                return NotFound();

            var (config, settings) = this.GetConfig();

            var settingProp = settings.FirstOrDefault(s => s.Name.ToLower() == setting.ToLower());
            if (settingProp == null)
                return NotFound();

            settingProp.SetValue(config, rate, null);
            config.AcceptedSpeed = rate;
            _context.Update(config);
            _context.SaveChanges();

            return Ok(rate);
        }

        [HttpGet]
        [Route("Condition/{condition}/{enabled}/{password?}")]
        public IActionResult SetSetting(string condition, bool enabled, string password = "")
        {
            if (_password != password || string.IsNullOrEmpty(condition))
                return NotFound();

            var conditionEntry = _context.ConditionSettings.FirstOrDefault(c => c.ConditionName == condition);
            if (conditionEntry == null)
                return NotFound();

            conditionEntry.Enabled = enabled;
            _context.Update(conditionEntry);
            _context.SaveChanges();

            return Ok(enabled);
        }

        [HttpGet]
        [Route("testnotifications/{password?}")]
        public IActionResult TestNotifications(string password = "")
        {
            if (_password != password)
                return NotFound();

            var notification = _serviceProvider.GetService<INotificationManager>();
            var result = notification.TriggerHook("Manual notification test, please ignore");
            if (result.HasFailed())
                return BadRequest();

            return Ok();
        }

        [HttpGet]
        [Route("version")]
        public IActionResult GetVersion()
        {
            var env = _serviceProvider.GetService<IHostingEnvironment>();
            var build = System.IO.File.ReadAllText("./version.txt");
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

            return Ok(new
            {
                BuildDate = build,
                Environment = env.EnvironmentName,
                Name = env.ApplicationName,
                Version = fvi.FileVersion
            });
        }

        private (ApiConfiguration config, List<PropertyInfo> configProps) GetConfig()
        {
            var config = _context.Configurations.OrderBy(o => o.ID).First();
            var type = config.GetType();
            var settings = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .ToList();
            settings.RemoveAll(p => p.Name.ToLower() == "id");

            return (config, settings);
        }
    }
}
