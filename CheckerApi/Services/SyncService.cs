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
        private readonly ILogger<SyncService> _logger;
        private readonly IMapper _mapper;
        private readonly INotificationManager _notification;
        private readonly IConditionComplier _condition;
        private readonly IAuditManager _audit;
        private readonly ApiContext _context;

        private readonly int[] _locations;
        private readonly int _alertInterval;
        private readonly string _alertMessage;
        private readonly string _url;
        private readonly string _request;

        public SyncService(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetService<ILogger<SyncService>>();
            _mapper = serviceProvider.GetService<IMapper>();
            _notification = serviceProvider.GetService<INotificationManager>();
            _condition = serviceProvider.GetService<IConditionComplier>();
            _audit = serviceProvider.GetService<IAuditManager>();
            _context = serviceProvider.GetService<ApiContext>();

            var config = serviceProvider.GetService<IConfiguration>();
            _alertInterval = config.GetValue<int>("Api:Alert:IntervalMin");
            _alertMessage = config.GetValue<string>("Api:Alert:Message");
            _url = config.GetValue<string>("NiceHash:Url");
            _request = config.GetValue<string>("NiceHash:Request");
            _locations = config.GetValue<int[]>("NiceHash:Locations");
        }

        public Result Run()
        {
            var swSync = Stopwatch.StartNew();
            _logger.LogInformation("Sync Started");

            try
            {
                var config = _context.ConfigurationReadOnly;
                var settings = _context.ConditionSettingsReadOnly.ToList();

                foreach (var location in _locations)
                {
                    var client = new RestClient(_url);
                    var request = new RestRequest(_request.Replace("{location}", location.ToString()), Method.GET);
                    var response = client.Execute(request);
                    var data = JsonConvert.DeserializeObject<ResultDTO>(response.Content);
                    var orders = data.Result.Orders.Select(o => CreateDTO(o, location)).ToList();

                    var auditOrders = orders.Where(o => o.Alive && o.AcceptedSpeed > 0).ToList();
                    _audit.CreateAudit(auditOrders);

                    var sw = Stopwatch.StartNew();
                    var foundOrders = _condition.Check(orders, config, settings).ToList();
                    sw.Stop();
                    var elapsed = sw.Elapsed;
                    _logger.LogInformation($"Conditions check took {elapsed.TotalSeconds} sec");

                    TriggerHooks(foundOrders);

                    if (foundOrders.Any())
                    {
                        _context.Data.AddRange(foundOrders.Select(b => b.BidEntry));
                        _context.SaveChanges();
                    }
                }

                swSync.Stop();
                var elapsedSync = swSync.Elapsed;
                _logger.LogInformation($"Sync Finished in {elapsedSync.TotalSeconds} sec");

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Sync failed: '{ex}'");
                return Result.Fail(ex.Message);
            }
        }

        private BidEntry CreateDTO(BidDTO order, int location)
        {
            var data = _mapper.Map<BidEntry>(order);
            data.NiceHashDataCenter = location;

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
    }
}