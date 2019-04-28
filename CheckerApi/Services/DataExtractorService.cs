using CheckerApi.Models;
using CheckerApi.Models.Config;
using CheckerApi.Models.Rpc;
using CheckerApi.Services.Interfaces;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace CheckerApi.Services
{
    public class DataExtractorService : IDataExtractorService
    {
        public Result<IEnumerable<string>> GetData(string url, string req, string pattern)
        {
            var client = new RestClient(url);
            var request = new RestRequest(req, Method.GET);
            var response = client.Execute(request);

            // Connection accepted, response was empty. Temporary networking issue
            if (response.StatusCode == 0)
            {
                return Result<IEnumerable<string>>.Ok(new List<string>());
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return Result<IEnumerable<string>>.Fail($"URL '{url}/{req}' returns status code '{response.StatusCode}'");
            }

            Regex expression = new Regex(pattern);
            Match match = expression.Match(response.Content);
            if (!match.Success)
            {
                return Result<IEnumerable<string>>.Fail("Regex not matched");
            }

            return Result<IEnumerable<string>>.Ok(match.Groups.Select(g => g.Value));
        }

        public Result<string> RpcCall(RpcConfig config, string method, params string[] parameters)
        {
            var result = this.RpcCall<RpcResult>(config, method, parameters);
            if (result.IsSuccess())
            {
                return Result<string>.Ok(result.Value.Result);
            }

            return Result<string>.Fail(result.Messages.ToArray());
        }

        public Result<T> RpcCall<T>(RpcConfig config, string method, params string[] parameters) where T : class
        {
            var client = new RestClient($"{config.Url}:{config.Port}");
            var request = new RestRequest(string.Empty, Method.POST)
            {
                Credentials = config.Credentials
            };

            var pars = string.Join(",", parameters.Select(p => $"\"{p}\""));
            request.AddParameter("text/xml", $"{{\"jsonrpc\":\"1.0\",\"id\":\"alert-bot\",\"method\":\"{method}\",\"params\":[{pars}]}}", ParameterType.RequestBody);
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return Result<T>.Fail($"RPC call '{method}' at '{config.Url}/{config.Port}' returns status code '{response.StatusCode}'");
            }

            try
            {
                var data = JsonConvert.DeserializeObject<T>(response.Content);
                return Result<T>.Ok(data);
            }
            catch (Exception ex)
            {
                return Result<T>.Fail($"RPC serialization fail '{method}' at '{config.Url}/{config.Port}'", $"object type: '{nameof(T)}'", $"ex: '{ex}'");
            }
        }
    }
}
