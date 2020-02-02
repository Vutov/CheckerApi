using CheckerApi.Models.Config;

namespace CheckerApi.Services.Interfaces
{
    public interface IForkWatchService
    {
        void Execute(RpcConfig rpcConfig);
    }
}
