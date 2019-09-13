using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using CheckerApi.Context;
using CheckerApi.Models;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace CheckerApi.Services
{
    public class SyncService : ISyncService
    {
        private readonly Dictionary<string, int> datacenters = new Dictionary<string, int>
        {
            { "EU", 0 },
            { "US", 1 }
        };

        private readonly ILogger<SyncService> _logger;
        private readonly IMapper _mapper;
        private readonly INotificationManager _notification;
        private readonly IConditionCompiler _condition;
        private readonly IAuditManager _audit;
        private readonly ApiContext _context;

        private readonly string[] _locations;
        private readonly int _alertInterval;
        private readonly string _alertMessage;
        private readonly string _url;
        private readonly string _request;

        public SyncService(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<SyncService>>();
            _mapper = serviceProvider.GetService<IMapper>();
            _notification = serviceProvider.GetService<INotificationManager>();
            _condition = serviceProvider.GetService<IConditionCompiler>();
            _audit = serviceProvider.GetService<IAuditManager>();
            _context = serviceProvider.GetService<ApiContext>();

            var config = serviceProvider.GetService<IConfiguration>();
            _alertInterval = config.GetValue<int>("Api:Alert:IntervalMin");
            _alertMessage = config.GetValue<string>("Api:Alert:Message");
            _url = config.GetValue<string>("NiceHash:Url");
            _request = config.GetValue<string>("NiceHash:Request");
            _locations = config.GetSection("NiceHash:Locations").Get<string[]>();
        }

        public Result RunSync()
        {
            try
            {
                var config = _context.ConfigurationReadOnly;
                var settings = _context.ConditionSettingsReadOnly.ToList();
                var totalOrders = GetTotalOrders(true);
                var poolData = _context.PoolHashratesReadOnly.ToList();

                var sw = Stopwatch.StartNew();
                var foundOrders = _condition.Check(totalOrders, config, settings, poolData).ToList();
                sw.Stop();
                var elapsed = sw.Elapsed;
                _logger.LogInformation($"Conditions check took {elapsed.TotalSeconds} sec");

                TriggerHooks(foundOrders);

                if (foundOrders.Any())
                {
                    _context.Data.AddRange(foundOrders.Select(b => b.BidEntry));
                    _context.SaveChanges();
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sync failed: '{ex}'");
                return Result.Fail(ex.Message);
            }
        }

        public Result RunHeartbeat()
        {
            try
            {
                var config = _context.ConfigurationReadOnly;
                var settings = _context.ConditionSettingsReadOnly.ToList();
                var totalOrders = this.GetTotalOrders(true);

                var sw = Stopwatch.StartNew();
                var heartbeats = _condition.GetHeartbeats(totalOrders, config, settings);
                sw.Stop();
                var elapsed = sw.Elapsed;
                _logger.LogInformation($"Heartbeat took {elapsed.TotalSeconds} sec");

                TriggerHeartbeat(heartbeats);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sync failed: '{ex}'");
                return Result.Fail(ex.Message);
            }
        }

        public IEnumerable<List<BidEntry>> GetTotalOrders(bool enableAudit)
        {
            var totalOrders = new List<List<BidEntry>>();
            foreach (var location in _locations)
            {
                int page = 0;
                var pagedList = new List<BidEntry>();
                do
                {
                    var client = new RestClient(_url);
                    var request = new RestRequest(_request.Replace("{location}", location.ToString()).Replace("{page}", page.ToString()), Method.GET);
                    var response = client.Execute(request);

                    // Bad response body
                    if (string.IsNullOrEmpty(response?.Content))
                    {
                        _logger.LogWarning("GetTotalOrders empty response body");
                        continue;
                    }

                    var data = JsonConvert.DeserializeObject<ResultDTO>(response.Content);
                    if (data?.Orders?.Any() == false)
                    {
                        break;
                    }

                    page++;
                    List<BidEntry> orders = data?.Orders?.Select(o => CreateDTO(o, location)).ToList() ?? new List<BidEntry>();
                    pagedList.AddRange(orders);
                } while (true);

                totalOrders.Add(pagedList);
                if (enableAudit)
                {
                    var auditOrders = pagedList?.Where(o => o.Alive && o.AcceptedSpeed > 0).ToList();
                    _audit.CreateAudit(auditOrders);
                }
            }

            return totalOrders;
        }

        private BidEntry CreateDTO(BidDTO order, string location)
        {
            var data = _mapper.Map<BidEntry>(order);

            int loc;
            if (datacenters.ContainsKey(location))
            {
                loc = datacenters[location];
            }
            else if (int.TryParse(location, out int l))
            {
                loc = l;
            } else
            {
                loc = 0;
            }

            data.NiceHashDataCenter = loc;

            return data;
        }

        private void TriggerHooks(IEnumerable<AlertDTO> alerts)
        {
            var config = _context.Configuration;
            foreach (var alert in alerts)
            {
                var alertMessage = string.Empty;
                if (DateTime.UtcNow.AddMinutes(-_alertInterval) >= config.LastNotification)
                {
                    alertMessage = _alertMessage;
                    config.LastNotification = DateTime.UtcNow;
                    _context.Configurations.Update(config);
                    _context.SaveChanges();
                }

                _notification.TriggerHook(alert.Message, alert.Condition, alertMessage);
            }
        }

        private void TriggerHeartbeat(IEnumerable<(string, string, string)> heartbeats)
        {
            foreach (var heartbeat in heartbeats)
            {
                _notification.TriggerHeartbeat(heartbeat.Item1, heartbeat.Item2, heartbeat.Item3);
            }
        }
    }
}