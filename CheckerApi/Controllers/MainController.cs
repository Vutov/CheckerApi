using System;
using System.Linq;
using CheckerApi.Context;
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

        public MainController(IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
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
            var config = _context.Configurations.First();
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

            var config = _context.Configurations.First();
            config.AcceptedSpeed = rate;
            _context.Update(config);
            _context.SaveChanges();

            return Ok(rate);
        }

        [HttpGet]
        [Route("acceptedspeed")]
        public IActionResult GetAcceptedSpeed(double rate)
        {
            return Ok(_context.Configurations.First().AcceptedSpeed);
        }

        [HttpGet]
        [Route("limitspeed/{rate}/{password?}")]
        public IActionResult SetLimitSpeed(double rate, string password = "")
        {
            if (_password != password)
                return NotFound();

            var config = _context.Configurations.First();
            config.LimitSpeed = rate;
            _context.Update(config);
            _context.SaveChanges();

            return Ok(rate);
        }

        [HttpGet]
        [Route("limitspeed")]
        public IActionResult GetLimitSpeed(double rate)
        {
            return Ok(_context.Configurations.First().LimitSpeed);
        }

        [HttpGet]
        [Route("pricethreshold/{rate}/{password?}")]
        public IActionResult SetPriceThreshold(double rate, string password = "")
        {
            if (_password != password)
                return NotFound();

            var config = _context.Configurations.First();
            config.PriceThreshold = rate;
            _context.Update(config);
            _context.SaveChanges();

            return Ok(rate);
        }

        [HttpGet]
        [Route("pricethreshold")]
        public IActionResult GetPriceThreshold(double rate)
        {
            return Ok(_context.Configurations.First().PriceThreshold);
        }
    }
}
