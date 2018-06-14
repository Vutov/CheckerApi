using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CheckerApi.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using ServiceStack;
using ServiceStack.Text;

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
            return Ok(Context.DataReadOnly.OrderByDescending(i => i.RecordDate).Take(top).ToList());
        }

        [HttpGet]
        [Route("audit.csv")]
        public IActionResult GetAuditOrdersCsv([FromQuery]string from, [FromQuery]string to, [FromQuery] string id, [FromQuery]int top = 1000)
        {
            var data = GetAudits(from, to, id, top).ToCsv();
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(data ?? ""));
            var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
            return File(stream, "text/csv", $"audit{timeStamp}.csv");
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
            IQueryable<BidAudit> baseQuery = Context.OrdersAuditsReadOnly.OrderByDescending(i => i.RecordDate);
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
