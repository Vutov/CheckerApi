using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CheckerApi.Context;
using CheckerApi.Models;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace CheckerApi.Services
{
    public class AuditManager: IAuditManager
    {
        private readonly ILogger<AuditManager> _logger;
        private readonly IMapper _mapper;
        private readonly ApiContext _context;

        public AuditManager(ILogger<AuditManager> logger, IMapper mapper, ApiContext context)
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
        }

        public async Task<Result> CreateAudit(IEnumerable<BidEntry> bids)
        {
            if (_context.Configuration.EnableAudit == false)
            {
                return Result.Ok();
            }

            try
            {
                var auditData = _mapper.Map<IEnumerable<BidAudit>>(bids);
                await _context.OrdersAudit.AddRangeAsync(auditData);
                await _context.SaveChangesAsync();

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
