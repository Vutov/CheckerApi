using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CheckerApi.Context;
using CheckerApi.Models.Config;
using CheckerApi.Models.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using RestSharp;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class PoolPullJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var poolsConfig = config.GetSection("Monitor:Pools").Get<IEnumerable<PoolConfig>>();

            foreach (var pool in poolsConfig)
            {
                if (!pool.Enabled)
                {
                    continue;
                }

                var logger = serviceProvider.GetService<ILogger<PoolPullJob>>();
                try
                {
                    var client = new RestClient(pool.Url);
                    var request = new RestRequest(Method.GET);
                    var response = client.Execute(request);
                    if (!response.IsSuccessful)
                    {
                        logger.LogError($"PoolPull failed: URL:'{pool.Url}', err:{response.Content}");
                    }

                    var context = serviceProvider.GetService<ApiContext>();
                    var content = response.Content;
                    foreach (var pattern in pool.Coins)
                    {
                        try
                        {
                            Regex expression = new Regex(pattern.Pattern);
                            Match match = expression.Match(content);
                            if (!match.Success)
                            {
                                logger.LogError($"PoolPull regex failed Name: '{pattern.Name}'");
                                continue;
                            }

                            var value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                            context.PoolHashrates.Add(new PoolHashrate()
                            {
                                Name = pattern.Name,
                                Value = value,
                                EntryDate = DateTime.UtcNow
                            });
                        }
                        catch (Exception ex)
                        {
                            logger.LogError($"PoolPull parse failed Name: '{pattern.Name}', error: '{ex}'");
                        }
                    }

                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.LogError($"PoolPull failed URL: '{pool.Url}', error: '{ex}'");
                }
            }
        }
    }
}
