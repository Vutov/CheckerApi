using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Context;
using CheckerApi.Filters;
using CheckerApi.Models.DTO;
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
    public class DebugController : BaseController
    {
        public DebugController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [AuthenticateFilter]
        [HttpGet]
        [Route("{condition}/{password}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public IActionResult GetCondition(string condition, string password)
        {
            var conditions = Registry.GetConditions().ToList();
            var conditionEntry = conditions.FirstOrDefault(c => c.Name.ToLower() == condition.ToLower());
            if (conditionEntry == null)
            {
                return NotFound();
            }

            var data = new List<AlertDTO>();
            var syncService = this.ServiceProvider.GetService<ISyncService>();
            var orders = syncService.GetTotalOrders(enableAudit: false);
            var config = this.Context.ConfigurationReadOnly;
            var poolData = this.Context.PoolHashratesReadOnly.ToList();

            ICondition conditionInstance = (ICondition)Activator.CreateInstance(conditionEntry, args: this.ServiceProvider);
            if (conditionEntry.IsDefined(typeof(GlobalConditionAttribute), false))
            {
                data = conditionInstance.Compute(orders.SelectMany(o => o), config, poolData).ToList();
            }
            else
            {
                foreach (var order in orders)
                {
                    data.AddRange(conditionInstance.Compute(order, config, poolData));
                }
            }

            var cache = ServiceProvider.GetService<IMemoryCache>();
            cache.TryGetValue<double>(Constants.HashRateKey, out var networkRate);
            cache.TryGetValue<double>(Constants.DifficultyKey, out var networkDifficulty);
            cache.TryGetValue<double>(Constants.BtcBtgPriceKey, out var price);

            return Ok(new
            {
                Condition = conditionEntry.FullName,
                Data = data,
                Variables = new
                {
                    StoredNetworkRate = networkRate,
                    StoredNetworkDifficulty = networkDifficulty,
                    StoredBtcBtgPrice = price
                },
                Input = new
                {
                    Config = config,
                    Orders = orders,
                    PoolData = poolData,
                }
            });
        }
    }
}
