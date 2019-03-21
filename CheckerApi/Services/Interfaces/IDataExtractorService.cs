using System.Collections.Generic;

namespace CheckerApi.Services.Interfaces
{
    public interface IDataExtractorService
    {
        IEnumerable<string> GetData(string url, string req, string pattern);
    }
}
