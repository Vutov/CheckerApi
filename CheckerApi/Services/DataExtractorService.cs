using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CheckerApi.Services.Interfaces;
using RestSharp;

namespace CheckerApi.Services
{
    public class DataExtractorService : IDataExtractorService
    {
        public IEnumerable<string> GetData(string url, string req, string pattern)
        {
            var client = new RestClient(url);
            var request = new RestRequest(req, Method.GET);
            var response = client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"URL '{url}/{req}' returns status code '{response.StatusCode}'");
            }

            Regex expression = new Regex(pattern);
            Match match = expression.Match(response.Content);
            if (!match.Success)
            {
                throw new InvalidOperationException("Regex not matched");
            }

            return match.Groups.Select(g => g.Value);
        }
    }
}
