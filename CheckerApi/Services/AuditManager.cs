using System;
using System.Collections.Generic;
using AutoMapper;
using CheckerApi.Context;
using CheckerApi.Models;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckerApi.Services
{
    public class AuditManager : IAuditManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditManager> _logger;
        private readonly IMapper _mapper;
        private readonly ApiContext _context;
        private readonly TimeSpan _recordThreshold;

        public AuditManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetService<ILogger<AuditManager>>();
            _mapper = serviceProvider.GetService<IMapper>();
            _context = _serviceProvider.GetService<ApiContext>();
            var config = serviceProvider.GetService<IConfiguration>();
            _recordThreshold = TimeSpan.FromMinutes(config.GetValue<int>("Api:ClearAuditMinutes"));
        }

        public Result CreateAudit(IEnumerable<BidEntry> bids)
        {
            if (_context.ConfigurationReadOnly.EnableAudit == false)
            {
                return Result.Ok();
            }

            try
            {
                var auditData = _mapper.Map<IEnumerable<BidAudit>>(bids);
                _context.OrdersAudit.AddRange(auditData);
                _context.SaveChanges();

                var recordThreshold = DateTime.UtcNow.Add(-_recordThreshold);

                // EF delete takes ages for large sets because of the tracking
                _context.Database.ExecuteSqlCommand($"DELETE FROM OrderAudits WHERE RecordDate <= @p0;", recordThreshold.ToString("yyyy-MM-dd hh:mm:ss")); 
                _context.SaveChanges();

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Save of Audit Data failed: {ex}");
                return Result.Fail();
            }
        }
    }
}
