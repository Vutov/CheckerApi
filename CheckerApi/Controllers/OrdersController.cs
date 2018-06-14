using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ServiceStack;

namespace CheckerApi.Controllers
{
    [Route("data")]
    public class OrdersController : BaseController
    {
        private readonly ICompressService _compresser;

        public OrdersController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _compresser = ServiceProvider.GetService<ICompressService>();
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
            var timeStamp = CreateTimeStamp();

            return File(stream, "text/csv", $"audit{timeStamp}.csv");
        }

        [HttpGet]
        [Route("audit")]
        public IActionResult GetAuditOrders([FromQuery]string from, [FromQuery]string to, [FromQuery] string id, [FromQuery]int top = 1000)
        {
            var data = GetAudits(from, to, id, top);
            return Ok(data);
        }

        [HttpGet]
        [Route("audit.zip")]
        public IActionResult Zip([FromQuery]string from, [FromQuery]string to, [FromQuery] string id, [FromQuery]int top = 1000)
        {
            var data = GetAudits(from, to, id, top).ToCsv();

            var timeStamp = CreateTimeStamp();
            var compressedBytes = _compresser.Zip(data, $"{timeStamp}.csv");

            return File(compressedBytes, "text/zip", $"audit{timeStamp}.zip");
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

        private string CreateTimeStamp()
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
        }
    }
}
