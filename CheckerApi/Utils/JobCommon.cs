using CheckerApi.Models.Config;
using Microsoft.Extensions.Configuration;
using System.Net;

public class JobCommon {
    public static RpcConfig GetRpcConfig (IConfiguration config)
    {
        return new RpcConfig()
        {
            Url = config.GetValue<string>("Node:Url"),
            Port = config.GetValue<int>("Node:RpcPort"),
            Credentials = new NetworkCredential()
            {
                UserName = config.GetValue<string>("Node:RpcUser"),
                Password = config.GetValue<string>("Node:RpcPass")
            }
        };
    }
}