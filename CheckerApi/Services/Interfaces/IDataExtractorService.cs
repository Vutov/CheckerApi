using System.Collections.Generic;
using CheckerApi.Models;

namespace CheckerApi.Services.Interfaces
{
    public interface IDataExtractorService
    {
        Result<IEnumerable<string>> GetData(string url, string req, string pattern);
    }
}
