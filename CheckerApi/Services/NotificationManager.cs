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
        private readonly ILogger<NotificationManager> _logger;
        private readonly string _triggerUri;
        private readonly string _triggerDomain;
        private readonly string _heartbeatDomain;
        private readonly string _heartbeatUri;

        public NotificationManager(IRestClient client, IConfiguration configuration, ILogger<NotificationManager> logger)
        {
            _client = client;
            _triggerDomain = configuration.GetValue<string>("Trigger:Domain");
            _triggerUri = configuration.GetValue<string>("Trigger:Uri");
            _heartbeatDomain = configuration.GetValue<string>("Heartbeat:Domain");
            _heartbeatUri = configuration.GetValue<string>("Heartbeat:Uri");
            _logger = logger;
        }

        public Result TriggerHook(params string[] messages)
        {
            return this.Trigger(_triggerDomain, _triggerUri, messages);
        }

        public Result TriggerHeartbeat(params string[] messages)
        {
            return this.Trigger(_heartbeatDomain, _heartbeatUri, messages);
        }

        private Result Trigger(string domain, string uri, params string[] messages)
        {
            _logger.LogTrace($"TriggerHook Messages: '{string.Join(",", messages)}'");

            try
            {
                _client.BaseUrl = new Uri(domain);
                var req = new RestRequest(uri, Method.POST);
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
