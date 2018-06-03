using System;
using System.Diagnostics;
using System.Linq;
using CheckerApi.Context;
using CheckerApi.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

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
            var config = _context.Configurations.OrderBy(o => o.ID).First();
            return Ok(new
            {
                Status = "Running",
                FoundOrders = _context.Data.Count(),
                Commands = new[] {
                        $"/acceptedspeed ({config.AcceptedSpeed})",
                        $"/limitspeed ({config.LimitSpeed})",
                        $"/pricethreshold ({config.PriceThreshold})",
                        "/data",
                        "/data/100"}
            });
        }

        [HttpGet]
        [Route("acceptedspeed/{rate}/{password?}")]
        public IActionResult SetAcceptedSpeed(double rate, string password = "")
        {
            if (_password != password)
                return NotFound();

            var config = _context.Configurations.OrderBy(o => o.ID).First();
            config.AcceptedSpeed = rate;
            _context.Update(config);
            _context.SaveChanges();

            return Ok(rate);
        }

        [HttpGet]
        [Route("acceptedspeed")]
        public IActionResult GetAcceptedSpeed(double rate)
        {
            return Ok(_context.Configurations.OrderBy(o => o.ID).First().AcceptedSpeed);
        }

        [HttpGet]
        [Route("limitspeed/{rate}/{password?}")]
        public IActionResult SetLimitSpeed(double rate, string password = "")
        {
            if (_password != password)
                return NotFound();

            var config = _context.Configurations.OrderBy(o => o.ID).First();
            config.LimitSpeed = rate;
            _context.Update(config);
            _context.SaveChanges();

            return Ok(rate);
        }

        [HttpGet]
        [Route("limitspeed")]
        public IActionResult GetLimitSpeed(double rate)
        {
            return Ok(_context.Configurations.OrderBy(o => o.ID).First().LimitSpeed);
        }

        [HttpGet]
        [Route("pricethreshold/{rate}/{password?}")]
        public IActionResult SetPriceThreshold(double rate, string password = "")
        {
            if (_password != password)
                return NotFound();

            var config = _context.Configurations.OrderBy(o => o.ID).First();
            config.PriceThreshold = rate;
            _context.Update(config);
            _context.SaveChanges();

            return Ok(rate);
        }

        [HttpGet]
        [Route("pricethreshold")]
        public IActionResult GetPriceThreshold(double rate)
        {
            return Ok(_context.Configurations.OrderBy(o => o.ID).First().PriceThreshold);
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
    }
}
