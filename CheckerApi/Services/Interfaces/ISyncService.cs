using CheckerApi.Models;

namespace CheckerApi.Services.Interfaces
{
    public interface ISyncService
    {
        Result RunSync();

        Result RunHeartbeat();
    }
}