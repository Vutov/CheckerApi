using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CheckerApi.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CheckerApi.Controllers
{
    [Route("data")]
    public class OrdersController : BaseController
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

        [HttpGet]
        [Route("audit.csv")]
        [Produces("text/csv")]
        public IActionResult GetAuditOrdersCsv([FromQuery]string from, [FromQuery]string to, [FromQuery] string id, [FromQuery]int top = 1000)
        {
            var data = GetAudits(from, to, id, top);
            return Ok(data);
        }

        [HttpGet]
        [Route("audit")]
        public IActionResult GetAuditOrders([FromQuery]string from, [FromQuery]string to, [FromQuery] string id, [FromQuery]int top = 1000)
        {
            var data = GetAudits(from, to, id, top);
            return Ok(data);
        }

        private List<BidAudit> GetAudits(string from, string to, string id, int top)
        {
            IQueryable<BidAudit> baseQuery = Context.OrdersAudit.OrderByDescending(i => i.RecordDate);
            if (!string.IsNullOrEmpty(id))
            {
                baseQuery = baseQuery.Where(r => r.NiceHashId == id);
            }

            if (!string.IsNullOrEmpty(from))
            {
                // ISO 8601
                baseQuery = baseQuery.Where(r => r.RecordDate >= DateTime.ParseExact(from, "s", CultureInfo.InvariantCulture));
            }

            if (!string.IsNullOrEmpty(to))
            {
                // ISO 8601
                baseQuery = baseQuery.Where(r => r.RecordDate <= DateTime.ParseExact(to, "s", CultureInfo.InvariantCulture));
            }
            
            var data = baseQuery.Take(top).ToList();
            return data;
        }
    }
}
