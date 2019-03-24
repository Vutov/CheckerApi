using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CheckerApi.Models;
using CheckerApi.Services.Interfaces;
using RestSharp;

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
    }
}
