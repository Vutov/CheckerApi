using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace CheckerApi.Controllers
{
    [Route("data")]
    public class OrdersController: BaseController
    {
        public OrdersController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [HttpGet]
        [Route("{top?}")]
        public IActionResult GetAlertOrders(int top = 10)
        {
            return Ok(Context.Data.OrderByDescending(i => i.RecordDate).Take(top).ToList());
        }

        // TODO 3 endpoints, return csv not json
        [HttpGet]
        [Route("audit/{top?}")]
        public IActionResult GetAuditOrders(int top = 1000)
        {
            var data = Context.OrdersAudit.OrderByDescending(i => i.RecordDate).Take(top).ToList();
            var json = JsonConvert.SerializeObject(data);
            byte[] bytes = Encoding.ASCII.GetBytes(json);
            var timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return File(bytes, "file/json", $"audit{timestamp}.txt");
        }
    }
}
