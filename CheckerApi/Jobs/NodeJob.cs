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
using System.Globalization;
using System.Linq.Expressions;
using System.Net;

namespace CheckerApi.Jobs
{
    [DisallowConcurrentExecution]
    public class NodeJob : Job
    {
        public override void Execute(JobDataMap data, IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetService<IConfiguration>();
            var dataExtractor = serviceProvider.GetService<IDataExtractorService>();
            var logger = serviceProvider.GetService<ILogger<NodeJob>>();
            var mapper = serviceProvider.GetService<IMapper>();

            var rpcConfig = JobCommon.GetRpcConfig(config);

            var cache = serviceProvider.GetService<IMemoryCache>();
            var difficultyResult = dataExtractor.RpcCall(rpcConfig, "getdifficulty");
            ProcessResult(logger, difficultyResult, diff =>
            {
                var difficulty = double.Parse(diff, CultureInfo.InvariantCulture);
                cache.Set(Constants.DifficultyKey, difficulty);
            });

            var networkRateResult = dataExtractor.RpcCall(rpcConfig, "getnetworkhashps");
            ProcessResult(logger, networkRateResult, rate =>
            {
                var networkRate = double.Parse(rate, CultureInfo.InvariantCulture);
                var networkRateInMh = DenominationHelper.ToMSol(networkRate, Denomination.Sol);
                cache.Set(Constants.HashRateKey, networkRateInMh);
            });

            var blockCount = config.GetValue<int>("Node:StoreLastBlocks");
            var chainTipResult = dataExtractor.RpcCall(rpcConfig, "getbestblockhash");
            ProcessResult(logger, chainTipResult, blockHash =>
            {
                var hasStoredBlocks = cache.TryGetValue(Constants.BlocksInfoKey, out BlocksList storedBlocks);
                if (!hasStoredBlocks)
                {
                    storedBlocks = new BlocksList();
                }

                if (storedBlocks.Contains(blockHash))
                {
                    // At chain tip
                    return;
                }

                ProcessResult(logger, GetBlockInfo(rpcConfig, dataExtractor, mapper, blockHash), (block) =>
                {
                    storedBlocks.Add(block);

                    Func<bool> expr;
                    if (hasStoredBlocks)
                    {
                        expr = () => !storedBlocks.Contains(block.PreviousBlockHash);
                    }
                    else
                    {
                        expr = () => storedBlocks.Count < blockCount;
                    }

                    while (expr())
                    {
                        ProcessResult(logger, GetBlockInfo(rpcConfig, dataExtractor, mapper, block.PreviousBlockHash), prevBlock =>
                            {
                                // We add from tip to genesis TimeSinceLast not present before here
                                storedBlocks.SetTimeSinceLast(block.Hash, block.Time - prevBlock.Time);

                                block = prevBlock;
                                storedBlocks.Add(prevBlock);
                            }
                        );
                    }
                });

                if (storedBlocks.Count > blockCount)
                {
                    storedBlocks.RemoveOver(blockCount);
                }

                cache.Set(Constants.BlocksInfoKey, storedBlocks);
            });
        }

        private Result<BlockInfoDTO> GetBlockInfo(RpcConfig rpcConfig, IDataExtractorService dataExtractor, IMapper mapper, string blockHash)
        {
            var blockResult = dataExtractor.RpcCall<RpcBlockResult>(rpcConfig, "getblock", blockHash);
            if (blockResult.HasFailed())
            {
                return Result<BlockInfoDTO>.Fail(blockResult.Messages.ToArray());
            }

            var block = mapper.Map<BlockInfoDTO>(blockResult.Value);
            return Result<BlockInfoDTO>.Ok(block);
        }

        private void ProcessResult<T>(ILogger<NodeJob> logger, Result<T> result, Action<T> action)
        {
            if (result.HasFailed())
            {
                logger.LogError(result.Messages.ToCommaSeparated());
                return;
            }

            action(result.Value);
        }
    }
}