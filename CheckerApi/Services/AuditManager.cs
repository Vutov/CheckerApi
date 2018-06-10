using System;
using System.Collections.Generic;
using AutoMapper;
using CheckerApi.Context;
using CheckerApi.Models;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CheckerApi.Services
{
    public class AuditManager : IAuditManager
    {
        private readonly ILogger<AuditManager> _logger;
        private readonly IMapper _mapper;
        private readonly ApiContext _context;

        public AuditManager(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<AuditManager>>();
            _mapper = serviceProvider.GetService<IMapper>();
            _context = serviceProvider.GetService<ApiContext>();
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
