using System;
using System.Collections.Generic;
using System.Linq;

namespace CheckerApi.Models.DTO
{
    public class BlocksList
    {
        private readonly Dictionary<string, BlockInfoDTO> _hashes;
        private List<BlockInfoDTO> _blocks;

        public BlocksList()
        {
            _blocks = new List<BlockInfoDTO>();
            _hashes = new Dictionary<string, BlockInfoDTO>();
        }

        public int Count
        {
            get
            {
                lock (_blocks)
                {
                    return _hashes.Count;
                }
            }
        }

        public bool Add(BlockInfoDTO block)
        {
            lock (_blocks)
            {
                if (_hashes.ContainsKey(block.Hash))
                {
                    return false;
                }

                if (_hashes.ContainsKey(block.PreviousBlockHash))
                {
                    var prev = _hashes[block.PreviousBlockHash];
                    block.TimeSinceLast = block.Time - prev.Time;
                }
                else
                {
                    block.TimeSinceLast = null;
                }

                _hashes.Add(block.Hash, block);
                _blocks.Add(block);

                return true;
            }
        }

        public void RemoveOver(int size)
        {
            lock (_blocks)
            {
                _blocks = _blocks.OrderByDescending(b => b.Height).ToList();
                while (_hashes.Count > size)
                {
                    var block = _blocks.Last();
                    _hashes.Remove(block.Hash);
                    _blocks.RemoveAt(_blocks.Count - 1);
                }
            }
        }

        public IEnumerable<BlockInfoDTO> GetSince(DateTime time)
        {
            lock (_blocks)
            {
                return _blocks.Where(b => b.Time > time).OrderBy(b => b.Height);
            }
        }

        public BlockInfoDTO GetTip()
        {
            lock (_blocks)
            {
                return _blocks.OrderBy(b => b.Height).Last();
            }
        }

        public bool Contains(string blockHash)
        {
            return _hashes.ContainsKey(blockHash);
        }

        public void SetTimeSinceLast(string blockHash, TimeSpan time)
        {
            if (_hashes.ContainsKey(blockHash))
            {
                _hashes[blockHash].TimeSinceLast = time;
                _blocks.First(b => b.Hash == blockHash).TimeSinceLast = time;
            }
        }
    }
}
