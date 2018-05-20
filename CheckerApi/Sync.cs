using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CheckerApi.DTO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

namespace CheckerApi
{
    public class Sync
    {
        private readonly ILogger<Sync> _logger;
        private readonly IMapper _mapper;
        public static HashSet<string> DataHashes = new HashSet<string>();
        private readonly IServiceProvider _serviceProvider;

        public Sync(ILogger<Sync> logger, IMapper mapper, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _mapper = mapper;
            _serviceProvider = serviceProvider;
        }

        public Task Run()
        {
            _logger.LogInformation("Sync Started");
            var locations = new[] { 0, 1 };

            return Task.Run(() =>
            {
                while (true)
                {
                    Task.Delay(TimeSpan.FromSeconds(15)).Wait();
                    using (var context = _serviceProvider.GetService<ApiContext>())
                    {
                        try
                        {
                            var config = context.Configurations.First();
                            foreach (var location in locations)
                            {
                                var client = new RestClient("https://api.nicehash.com/");
                                var request = new RestRequest(
                                    $"api?method=orders.get&location={location}&algo=24",
                                    Method.GET);
                                var response = client.Execute(request);
                                var data = JsonConvert.DeserializeObject<ResultDTO>(response.Content);

                                var orders = data.Result.Orders;
                                var foundOrders = new List<DataDB>();
                                foundOrders.AddRange(AcceptedSpeedCondition(orders, location, config));
                                foundOrders.AddRange(SignOfAttack(orders, location, config));
                                if (foundOrders.Any())
                                {
                                    TriggerHook(context);
                                    context.Data.AddRange(foundOrders);
                                    context.SaveChanges();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.ToString());
                        }
                    }
                }
            });
        }

        private List<DataDB> AcceptedSpeedCondition(IEnumerable<DataDTO> orders, int location, Configuration config)
        {
            var foundOrders = new List<DataDB>();
            foreach (var order in orders)
            {
                var ordStr = JsonConvert.SerializeObject(order);
                var hash = Sha256(ordStr);

                if (!DataHashes.Contains(hash) &&
                    order.Alive &&
                    order.AcceptedSpeed >= config.AcceptedSpeed
                )
                {
                    DataHashes.Add(hash);
                    foundOrders.Add(CreateDTO(order, location));
                    _logger.LogInformation(ordStr);
                }
            }

            return foundOrders;
        }

        private List<DataDB> SignOfAttack(IEnumerable<DataDTO> orders, int location, Configuration config)
        {
            var foundOrders = new List<DataDB>();
            var top2 = orders.Where(o => o.Alive).OrderByDescending(o => o.Price).Take(2).ToList();
            if (top2[0].Price + config.PriceThreshold >= top2[1].Price &&
               (top2[0].LimitSpeed == 0 || top2[0].LimitSpeed >= config.LimitSpeed)
            )
            {
                var order = top2.First();
                var ordStr = JsonConvert.SerializeObject(order);
                var hash = Sha256(ordStr);

                DataHashes.Add(hash);
                foundOrders.Add(CreateDTO(order, location));
                _logger.LogInformation(ordStr);
            }

            return foundOrders;
        }

        private DataDB CreateDTO(DataDTO order, int location)
        {
            var data = _mapper.Map<DataDB>(order);
            data.NiceHashDataCenter = location;

            return data;
        }

        private void TriggerHook(ApiContext context)
        {
            var config = context.Configurations.First();
            if (DateTime.UtcNow.AddMinutes(-5) >= config.LastNotification)
            {
                var hook = new RestClient("http://maker.ifttt.com");
                var req = new RestRequest(Storage.Trigger, Method.GET);
                var res = hook.Execute(req);
                _logger.LogInformation(res.Content);
                config.LastNotification = DateTime.UtcNow;
                context.Configurations.Update(config);
                context.SaveChanges();
            }
        }

        private string Sha256(string randomString)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}