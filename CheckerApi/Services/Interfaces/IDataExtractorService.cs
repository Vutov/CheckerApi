using CheckerApi.Models;
using System.Collections.Generic;
using CheckerApi.Models.Config;

namespace CheckerApi.Services.Interfaces
{
    public interface IDataExtractorService
    {
        Result<IEnumerable<string>> GetData(string url, string req, string pattern);
        Result<T> RpcCall<T>(RpcConfig config, string method, params string[] parameters) where T : class;
        Result<string> RpcCall(RpcConfig config, string method, params string[] parameters);
    }
}
