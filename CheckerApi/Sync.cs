using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace CheckerApi
{
    public class Sync
    {
        public Task Run()
        {

            var locations = new[] { 0, 1 };

            return Task.Run(() =>
                {
                    while (true)
                    {
                        Task.Delay(TimeSpan.FromSeconds(15)).Wait();
                        try
                        {
                            foreach (var location in locations)
                            {
                                var client = new RestClient("https://api.nicehash.com/");
                                var request = new RestRequest($"api?method=orders.get&location={location}&algo=24", Method.GET);
                                var response = client.Execute(request);
                                var data = JsonConvert.DeserializeObject<Result>(response.Content);

                                var orders = data.result.orders;

                                foreach (var order in orders)
                                {
                                    AcceptedSpeedCondition(order);
                                }

                                var top2 = orders.Where(o => o.alive).OrderByDescending(o => o.price).Take(2).ToList();
                                SignOfAttack(top2);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            );
        }

        private void SignOfAttack(List<Data> top2)
        {
            if (top2[0].price + Storage.PriceThreshold >= top2[1].price &&
               (top2[0].limit_speed == 0 || top2[0].limit_speed >= Storage.LimitSpeed)
            )
            {
                var order = top2.First();
                var ordStr = JsonConvert.SerializeObject(order);
                var hash = sha256(ordStr);

                TriggerHook();
                Storage.Data.Add(hash, order);
                Storage.ExtendedData.Add(new ExtendedData()
                {
                    Data = order,
                    DateTime = DateTime.UtcNow
                });
                Console.WriteLine(ordStr);
            }
        }

        private void AcceptedSpeedCondition(Data order)
        {
            var ordStr = JsonConvert.SerializeObject(order);
            var hash = sha256(ordStr);

            if (!Storage.Data.ContainsKey(hash) &&
                order.alive &&
                order.accepted_speed >= Storage.AcceptedSpeed
            )
            {
                TriggerHook();
                Storage.Data.Add(hash, order);
                Storage.ExtendedData.Add(new ExtendedData()
                {
                    Data = order,
                    DateTime = DateTime.UtcNow
                });
                Console.WriteLine(ordStr);
            }
        }

        private static void TriggerHook()
        {
            if (DateTime.UtcNow >= Storage.LastNotification)
            {
                var hook = new RestClient("http://maker.ifttt.com");
                var req = new RestRequest(Storage.Trigger, Method.GET);
                var res = hook.Execute(req);
                Console.WriteLine(res.Content);
                Storage.LastNotification = DateTime.UtcNow.AddMinutes(2);
            }

        }

        string sha256(string randomString)
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


    class Result
    {
        public Orders result { get; set; }
    }

    class Orders
    {
        public IEnumerable<Data> orders { get; set; }
    }

    public class Data
    {
        public double limit_speed { get; set; }
        public bool alive { get; set; }
        public double price { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string workers { get; set; }
        public string algo { get; set; }
        public double accepted_speed { get; set; }
    }
}

