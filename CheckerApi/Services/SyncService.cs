using System;
using System.Collections.Generic;
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

        private readonly int[] _locations = { 0, 1 };
        private readonly int _alertInterval;
        private readonly string _alertMessage;

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
        }

        public Result Run()
        {
            _logger.LogInformation("Sync Started");

            try
            {
                var config = _context.ConfigurationReadOnly;
                var settings = _context.ConditionSettingsReadOnly.ToList();

                foreach (var location in _locations)
                {
                    var client = new RestClient("https://api.nicehash.com/");
                    var request = new RestRequest($"api?method=orders.get&location={location}&algo=24", Method.GET);
                    var response = client.Execute(request);
                    var data = JsonConvert.DeserializeObject<ResultDTO>(response.Content);
                    var orders = data.Result.Orders.Select(o => CreateDTO(o, location)).ToList();

                    _audit.CreateAudit(orders);

                    var foundOrders = _condition.Check(orders, config, settings).ToList();
                    TriggerHooks(foundOrders);

                    if (foundOrders.Any())
                    {
                        _context.Data.AddRange(foundOrders.Select(b => b.BidEntry));
                        _context.SaveChanges();
                    }
                }

                _logger.LogInformation("Sync Finished");
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