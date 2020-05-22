using CheckerApi.Extensions;
using CheckerApi.Models;
using CheckerApi.Models.Config;
using CheckerApi.Models.DTO;
using CheckerApi.Models.Rpc;
using CheckerApi.Services.Interfaces;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckerApi.Services
{
    public class ForkWatchService: IForkWatchService
    {
        private const int VIRTUALFINALIZEBLOCKS = 3;

        private IDataExtractorService _dataExtractor;
        private ILogger<ForkWatchService> _logger;
        private IMemoryCache _cache;
        private INotificationManager _notificationManager;

        public ForkWatchService(IDataExtractorService dataExtractor, IMemoryCache cache, INotificationManager notificationManager)
        {
            _dataExtractor = dataExtractor;
            _cache = cache;
            _notificationManager = notificationManager;
        }

        public void Execute(RpcConfig rpcConfig)
        {
            // Compare chain tips
            var lastSeenTips = _cache.GetOrCreate<ChainTip[]>(Constants.LastSeenTipKey, entry => null);
            ChainTip[] tips = null;
            Handle(_dataExtractor.RpcCall<GetChainTipsResult>(rpcConfig, "getchaintips"), r =>
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
                    var url = string.Empty;  // TODO: upload to pastbin and get url
                    var message = $"{desc}\n{url}";
                    _notificationManager.TriggerHook(message);
                }

                _cache.Set(Constants.LastSeenTipKey, tips);
            });

            if (tips == null || tips.Length == 0)
            {
                _logger.LogWarning("ForkWatch: Got malformed tip from RPC");
            }

            if (lastSeenTips != null && lastSeenTips[0].Hash == tips[0].Hash)
            {
                // Short-circurit: no new block found
                return;
            }

            // Check virtual checkpoint rolled-back
            var lastCheckpoint = _cache.GetOrCreate<VirtualCheckpointDTO>(Constants.VirtualCheckpointKey, entry => null);
            if (lastCheckpoint != null)
            {
                bool foundReorg = false;

                // Check if the checkpoint is still in the main chain
                Handle(_dataExtractor.RpcCall<RpcResult>(rpcConfig, "getblockhash", lastCheckpoint.Height), r =>
                {
                    var hash = r.Result;
                    foundReorg = hash != lastCheckpoint.Hash;
                    var message = $"ForkWatch: Virtual checkpoint {lastCheckpoint.Hash} at height {lastCheckpoint.Height} replaced by {hash}";
                    _notificationManager.TriggerHook(message);
                });

                // TODO: if foundReorg, move to "PREPARE"
            }

            // Update virtual checkpoint
            var height = tips[0].Height;
            var toFinalize = height - VIRTUALFINALIZEBLOCKS;

            _logger.LogWarning("getblockhash({0})", toFinalize);

            Handle(_dataExtractor.RpcCall<RpcResult>(rpcConfig, "getblockhash", toFinalize), r =>
            {
                var hash = r.Result;
                _cache.Set(Constants.VirtualCheckpointKey, new VirtualCheckpointDTO()
                {
                    Hash = hash,
                    Height = toFinalize
                });

                _notificationManager.TriggerHook($"ForkWatch: new checkpoint {hash} at {toFinalize} ({-VIRTUALFINALIZEBLOCKS}) tip: {height}");
            });
        }

        private void Handle<T>(Result<T> result, Action<T> action)
        {
            if (result.HasFailed())
            {
                _logger.LogError(result.Messages.ToCommaSeparated());
                return;
            }

            action(result.Value);
        }

        private (bool found, string payload) DiffTip(IEnumerable<ChainTip> a, IEnumerable<ChainTip> b)
        {
            var hashesA = a.Where(t => t.Status != "active").Select(t => t.Hash).ToHashSet();
            var dictB = b.Where(t => t.Status != "active" && t.Status != "headers-only").ToDictionary(k => k.Hash, v => v);
            var hashesB = dictB.Select(t => t.Key).ToHashSet();
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
        private List<RpcBlockInfo> BacktraceBlocks(RpcConfig rpcConfig, string tipHash, int n = 0)
        {
            var blocks = new List<RpcBlockInfo>();
            var h = tipHash;
            while (true)
            {
                var blockInfoResult = _dataExtractor.RpcCall<RpcBlockInfo>(rpcConfig, "getblock", h);
                if (blockInfoResult.HasFailed())
                {
                    _logger.LogError(blockInfoResult.Messages.ToCommaSeparated());
                    break;
                }

                var blockInfo = blockInfoResult.Value;
                _logger.LogInformation("Backtrace: {0} {1}", h, blockInfo.Confirmations);
                if (n == 0 && blockInfo.Confirmations >= 0)
                {
                    break;
                }

                blocks.Append(blockInfo);
                h = blockInfo.PreviousBlockHash;
                if (blocks.Count == n)
                {
                    break;
                }
            }

            return blocks;
        }
    }
}
