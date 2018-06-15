using System;
using CheckerApi.Models;
using CheckerApi.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace CheckerApi.Services
{
    public class NotificationManager : INotificationManager
    {
        private readonly IRestClient _client;
        private readonly string _domain;
        private readonly string _uri;
        private readonly ILogger<NotificationManager> _logger;

        public NotificationManager(IRestClient client, IConfiguration configuration, ILogger<NotificationManager> logger)
        {
            _client = client;
            _domain = configuration.GetValue<string>("Trigger:Domain");
            _uri = configuration.GetValue<string>("Trigger:Uri");
            _logger = logger;
        }

        public Result TriggerHook(params string[] messages)
        {
            _logger.LogTrace($"TriggerHook Messages: '{string.Join(",", messages)}'");

            try
            {
                _client.BaseUrl = new Uri(_domain);
                var req = new RestRequest(_uri, Method.POST);
                for (int i = 0; i < messages.Length; i++)
                {
                    var message = messages[i];

                    // value1, value2 and value3 are the params for IFTTT webhook
                    req.AddParameter($"value{i + 1}", message, ParameterType.GetOrPost);
                }

                var res = _client.Execute(req);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Send Notification failed: '{ex}'");
                return Result.Fail();
            }
        }
    }
}
