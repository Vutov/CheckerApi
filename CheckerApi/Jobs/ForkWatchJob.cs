using AutoMapper;
using CheckerApi.Extensions;
using CheckerApi.Models;
using CheckerApi.Models.Config;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Rpc;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class ForkWatchJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var executor = new WatchJobExecutor()
            {
                config = serviceProvider.GetService<IConfiguration>(),
                dataExtractor = serviceProvider.GetService<IDataExtractorService>(),
                logger = serviceProvider.GetService<ILogger<ForkWatchJob>>(),
                mapper = serviceProvider.GetService<IMapper>(),
                cache = serviceProvider.GetService<IMemoryCache>(),
                notificationManager = serviceProvider.GetService<INotificationManager>(),
            };
            executor.Execute();
        }
    }

    class VirtualCheckpoint
    {
        public int Height { get; set; }
        public string Hash { get; set; }
    }

    class WatchJobExecutor
    {
        const int VIRTUAL_FINALIZE_BLOCKS = 3;

        public IConfiguration config;
        public IDataExtractorService dataExtractor;
        public ILogger<ForkWatchJob> logger;
        public IMapper mapper;
        public IMemoryCache cache;
        public RpcConfig rpcConfig;
        public INotificationManager notificationManager;

        public void Execute()
        {
            rpcConfig = Job.GetRpcConfig(config);
            // compare chain tips
            var lastSeenTips = cache.GetOrCreate<ChainTip[]>(Constants.LastSeenTipKey, entry => null);
            ChainTip[] tips = null;
            var shouldBacktrace = false;
            Handle(Rpc<GetChainTipsResult>("getchaintips"), r =>
            {
                tips = r.Result;
                var shouldSend = true;
                var desc = "ForkWatch started";
                if (lastSeenTips != null)
                {
                    (shouldSend, desc) = DiffTip(lastSeenTips, tips);
                }
                if (shouldSend)
                {
                    shouldBacktrace = true;
                    var url = "";  // TODO: upload to pastbin and get url
                    var message = $"{desc}\n{url}";
                    notificationManager.TriggerHook(message);
                }

                cache.Set(Constants.LastSeenTipKey, tips);
            });

            if (tips == null || tips.Length == 0)
            {
                logger.LogWarning("ForkWatch: Got bad tip");
            }

            // short-circurit: no new block found
            if (lastSeenTips != null && lastSeenTips[0].Hash == tips[0].Hash)
            {
                return;
            }

            // check virtual checkpoint rolled-back
            var lastCheckpoint = cache.GetOrCreate<VirtualCheckpoint>(
                Constants.VirtualCheckpointKey, entry => null);
            if (lastCheckpoint != null)
            {
                bool foundReorg = false;
                // check if the checkpoint is still in the main chain
                Handle(Rpc<RpcResult>("getblockhash", lastCheckpoint.Height), r =>
                {
                    var hash = r.Result;
                    foundReorg = (hash != lastCheckpoint.Hash);
                    var message = $"ForkWatch: Virtual checkpoint {lastCheckpoint.Hash} at height {lastCheckpoint.Height} replaced by {hash}";
                    notificationManager.TriggerHook(message);
                });
                // TODO: if foundReorg, move to "PREPARE"
            }
            // update virtual checkpoint
            var height = tips[0].Height;
            var toFinalize = height - VIRTUAL_FINALIZE_BLOCKS;
            logger.LogWarning("getblockhash({0})", toFinalize);
            Handle(Rpc<RpcResult>("getblockhash", toFinalize), r =>
            {
                var hash = r.Result;
                cache.Set(Constants.VirtualCheckpointKey, new VirtualCheckpoint()
                {
                    Hash = hash,
                    Height = toFinalize
                });
                notificationManager.TriggerHook(
                    $"ForkWatch: new checkpoint {hash} at {toFinalize} ({-VIRTUAL_FINALIZE_BLOCKS}) tip: {height}");
            });
        }

        Result<T> Rpc<T>(string name, params object[] args) where T : class
        {
            return dataExtractor.RpcCall<T>(rpcConfig, name, args);
        }

        Result<RpcResult> Rpc(string name, params object[] args)
        {
            return Rpc<RpcResult>(name, args);
        }

        void Handle<T>(Result<T> result, Action<T> action)
        {
            if (result.HasFailed())
            {
                logger.LogError(result.Messages.ToCommaSeparated());
                return;
            }
            action(result.Value);
        }

        (bool, string) DiffTip(ChainTip[] a, ChainTip[] b)
        {
            var dictA = (from t in a where t.Status != "active" select t)
                .ToDictionary(t => t.Hash, t => t);
            var dictB = (from t in b where t.Status != "active" && t.Status != "headers-only" select t)
                .ToDictionary(t => t.Hash, t => t);

            var hashesA = new HashSet<string>(dictA.Keys);
            var hashesB = new HashSet<string>(dictB.Keys);
            var added = new HashSet<string>(hashesB);
            added.ExceptWith(hashesA);

            if (!added.Any())
            {
                return (false, null);
            }

            var branches = SimpleJson.SimpleJson.SerializeObject(
                (from h in added select dictB[h]).ToList());
            return (true, $"New branch: {branches}");
        }

        // n == 0: backtrace until meet main chain
        // n > 0: backtrace n blocks
        List<RpcBlockInfo> BacktraceBlocks(string tipHash, int n = 0)
        {
            var blocks = new List<RpcBlockInfo>();
            var h = tipHash;
            while (true)
            {
                var end = false;
                Handle(Rpc<RpcBlockInfo>("getblock", h), b =>
                {
                    logger.LogInformation("Backtrace: {0} {1}", h, b.Confirmations);
                    if (n == 0 && b.Confirmations >= 0)
                    {
                        end = true;
                        return;
                    }
                    blocks.Append(b);
                    h = b.PreviousBlockHash;
                    if (blocks.Count == n)
                    {
                        end = true;
                    }
                });
                if (end) break;
            }
            return blocks;
        }
    }
}