using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace CheckerApi
{
    [Route("")]
    public class ValuesController : Controller
    {
        private readonly string _password;

        public ValuesController()
        {
            _password = Storage.Password;
        }

        [HttpGet]
        [Route("data/{top}")]
        public IActionResult Get(int top)
        {
            return Ok(Storage.ExtendedData.TakeLast(top));
        }

        [HttpGet]
        [Route("data")]
        public IActionResult Get10(int top)
        {
            return Ok(Storage.ExtendedData.TakeLast(10));
        }

        [HttpGet]
        [Route("")]
        public IActionResult Status()
        {
            return Ok(new
            {
                Status = "Running",
                FoundOrders = Storage.Data.Count,
                Commands = new[] {
                        $"/acceptedspeed ({Storage.AcceptedSpeed})",
                        $"/limitspeed ({Storage.LimitSpeed})",
                        $"/pricethreshold ({Storage.PriceThreshold})",
                        "/data",
                        "/data/100"}
            });
        }

        [HttpGet]
        [Route("acceptedspeed/{rate}/{password}")]
        public IActionResult SetAcceptedSpeed(double rate, string password)
        {
            if (_password != password)
                return NotFound();
            return Ok(Storage.AcceptedSpeed = rate);
        }

        [HttpGet]
        [Route("acceptedspeed")]
        public IActionResult GetAcceptedSpeed(double rate)
        {
            return Ok(Storage.AcceptedSpeed);
        }

        [HttpGet]
        [Route("limitspeed/{rate}/{password}")]
        public IActionResult SetLimitSpeed(double rate, string password)
        {
            if (_password != password)
                return NotFound();
            return Ok(Storage.LimitSpeed = rate);
        }

        [HttpGet]
        [Route("limitspeed")]
        public IActionResult GetLimitSpeed(double rate)
        {
            return Ok(Storage.LimitSpeed);
        }

        [HttpGet]
        [Route("pricethreshold/{rate}/{password}")]
        public IActionResult SetPriceThreshold(double rate, string password)
        {
            if (_password != password)
                return NotFound();
            return Ok(Storage.PriceThreshold = rate);
        }

        [HttpGet]
        [Route("pricethreshold")]
        public IActionResult GetPriceThreshold(double rate)
        {
            return Ok(Storage.PriceThreshold);
        }

        [HttpGet]
        [Route("restart/{password}")]
        public IActionResult Restart(double rate, string password)
        {
            if (_password != password)
                return NotFound();
            Storage.ExtendedData = new List<ExtendedData>();
            Storage.Data = new Dictionary<string, Data>();
            return Ok();
        }
    }
}
