using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CheckerApi.Context;
using CheckerApi.Models;
using CheckerApi.Models.Config;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace CheckerApi.Services
{
    public class PoolPullService : IPoolPullService
    {
        private readonly ILogger<SyncService> _logger;
        private readonly ApiContext _context;
        private readonly IEnumerable<PoolConfig> _poolsConfig;

        public PoolPullService(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<SyncService>>();
            _context = serviceProvider.GetService<ApiContext>();

            var config = serviceProvider.GetService<IConfiguration>();
            _poolsConfig = config.GetSection("Monitor:Pools").Get<IEnumerable<PoolConfig>>();
        }

        public Result RunPull()
        {
            foreach (var pool in _poolsConfig)
            {
                if (!pool.Enabled)
                {
                    continue;
                }

                try
                {
                    var client = new RestClient(pool.Url);
                    var request = new RestRequest(Method.GET);
                    var response = client.Execute(request);
                    if (!response.IsSuccessful)
                    {
                        _logger.LogError($"PoolPull failed: URL:'{pool.Url}', err: '{response.Content}'");
                    }

                    var content = response.Content;
                    foreach (var pattern in pool.Coins)
                    {
                        var result = this.GetHashrateEntity(content, pattern);
                        if (result.IsSuccess())
                        {
                            _context.PoolHashrates.Add(result.Value);
                        }
                        else
                        {
                            _logger.LogError(string.Join(",", result.Messages));
                        }
                    }

                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"PoolPull failed URL: '{pool.Url}', error: '{ex}'");
                }
            }

            return Result.Ok();
        }

        private Result<PoolHashrate> GetHashrateEntity(string content, CoinConfig pattern)
        {
            try
            {
                Regex expression = new Regex(pattern.Pattern);
                Match match = expression.Match(content);
                if (!match.Success)
                {
                    return Result<PoolHashrate>.Fail($"PoolPull regex failed Name: '{pattern.Name}'");
                }

                var value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                var denomination = pattern.Denomination;
                if (match.Groups[2].Success)
                {
                    denomination = DenominationHelper.ToDenomination(match.Groups[2].Value);
                }

                var mSolValue = DenominationHelper.ToMSol(value, denomination);
                var poolHashrate = new PoolHashrate()
                {
                    Name = pattern.Name,
                    Value = mSolValue,
                    EntryDate = DateTime.UtcNow
                };

                return Result<PoolHashrate>.Ok(poolHashrate);
            }
            catch (Exception ex)
            {
                return Result<PoolHashrate>.Fail($"PoolPull parse failed Name: '{pattern.Name}', error: '{ex}'");
            }
        }
    }
}
