using System;
using System.Linq;
using CheckerApi.Filters;
using CheckerApi.Services.Conditions;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace CheckerApi.Controllers
{
    [Route("Debug")]
    [Produces("application/json")]
    public class ConditionController : BaseController
    {
        public ConditionController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [AuthenticateFilter]
        [HttpGet]
        [Route("MarketCondition")]
        public IActionResult TriggerMarketCondition(string password)
        {
            var syncService = this.ServiceProvider.GetService<ISyncService>();
            var condition = new TotalMarketCondition(ServiceProvider);
            var orders = syncService.GetTotalOrders(enableAudit: false);
            var config = this.Context.ConfigurationReadOnly;
            var poolData = this.Context.PoolHashratesReadOnly.ToList();
            var data = condition.Compute(orders.SelectMany(o => o), config, poolData).ToList();

            var cache = ServiceProvider.GetService<IMemoryCache>();
            cache.TryGetValue<double>(Constants.HashRateKey, out var networkRate);
            cache.TryGetValue<double>(Constants.DifficultyKey, out var networkDifficulty);
            cache.TryGetValue<double>(Constants.BtcBtgPriceKey, out var price);

            return Ok(new
            {
                Data = data,
                Variables = new
                {
                    StoredNetworkRate = networkRate,
                    StoredNetworkDifficulty = networkDifficulty,
                    StoredBtcBtgPrice = price
                },
                Input = new
                {
                    PoolData = poolData,
                    Orders = orders,
                    Config = config
                }
            });
        }
    }
}
