using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Conditions;
using CheckerApi.Utils;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;

namespace CheckerApi.UnitTests.Services.Conditions
{
    [TestFixture]
    public class TotalMarketConditionTests
    {
        private Mock<IServiceProvider> _serviceProvider;
        private Mock<IMemoryCache> _cache;

        [SetUp]
        public void Setup()
        {
            _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            _cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            object value = 10000d;
            _cache.Setup(s => s.TryGetValue(Constants.HashRateKey, out value)).Returns(true);
            _serviceProvider.Setup(x => x.GetService(typeof(IMemoryCache)))
                .Returns(_cache.Object);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Compute_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
        {
            // Arrange
            var compiler = new TotalMarketCondition(_serviceProvider.Object);
            var config = new ApiConfiguration()
            {
                TotalHashThreshold = 0.8
            };

            // Act
            var data = compiler.Compute(GetSignBidSet(id), config);

            // Assert
            Assert.IsFalse(data.Any());
        }

        [Test]
        public void Compute_ShouldReturnBid_WhenMetCondition()
        {
            // Arrange
            var compiler = new TotalMarketCondition(_serviceProvider.Object);

            // Total AcceptedSpeed = 45, Network Speed = 10
            var orders = GetMarketThresholdAttackMetConditionBids();

            var config = new ApiConfiguration()
            {
                TotalHashThreshold = 1
            };

            // Act
            var data = compiler.Compute(orders, config).OrderBy(o => o.BidEntry.NiceHashId).ToList();

            // Asserts
            Assert.AreEqual(1, data.Count());
            Assert.IsNotEmpty("0", data[0].BidEntry.NiceHashId);
            Assert.AreEqual(0, data[0].BidEntry.NiceHashDataCenter);
            Assert.IsTrue(data.First().Message.Contains("AT RISK"));
        }

        private IEnumerable<BidEntry> GetSignBidSet(int id)
        {
            var data = new Dictionary<int, IEnumerable<BidEntry>>()
                {
                    {
                        1, new List<BidEntry>()
                        {
                            new BidEntry()
                            {
                                NiceHashId = "1",
                                Alive = true,
                                AcceptedSpeed = 0.5,
                                Price = 12
                            }
                        }
                    },
                    {
                        2, new List<BidEntry>()
                        {
                            new BidEntry()
                            {
                                NiceHashId = "3",
                                Alive = true,
                                AcceptedSpeed = 0.5,
                                Price = 5
                            },
                            new BidEntry()
                            {
                                NiceHashId = "4",
                                Alive = true,
                                AcceptedSpeed = 0.0,
                                Price = 1.1
                            }
                        }
                    },
                    {
                        3, new List<BidEntry>()
                    }
                };

            return data[id];
        }

        private IEnumerable<BidEntry> GetMarketThresholdAttackMetConditionBids()
        {
            return new List<BidEntry>()
                {
                    new BidEntry()
                    {
                        NiceHashId = "15",
                        Price = 1,
                        Alive = true,
                        AcceptedSpeed = 9,
                        LimitSpeed = 1
                    },
                    new BidEntry()
                    {
                        NiceHashId = "16",
                        Price = 2,
                        Alive = true,
                        AcceptedSpeed = 1,
                        LimitSpeed = 0
                    },
                    new BidEntry()
                    {
                        NiceHashId = "17",
                        Price = 4,
                        Alive = true,
                        AcceptedSpeed = 10,
                        LimitSpeed = 14
                    },
                    new BidEntry()
                    {
                        NiceHashId = "18",
                        Price = 5,
                        Alive = true,
                        AcceptedSpeed = 12,
                        LimitSpeed = 18
                    },
                    new BidEntry()
                    {
                        NiceHashId = "19",
                        Price = 10,
                        Alive = true,
                        AcceptedSpeed = 13,
                        LimitSpeed = 0
                    },
                    new BidEntry()
                    {
                        NiceHashId = "25",
                        Price = 11,
                        Alive = true,
                        AcceptedSpeed = 0.2,
                        LimitSpeed = 5
                    },
                    new BidEntry()
                    {
                        NiceHashId = "20",
                        Price = 2,
                        Alive = true,
                        AcceptedSpeed = 0.1,
                        LimitSpeed = 10
                    },
                    new BidEntry()
                    {
                        NiceHashId = "21",
                        Price = 2,
                        Alive = false,
                        AcceptedSpeed = 140,
                        LimitSpeed = 1
                    },
                    new BidEntry()
                    {
                        NiceHashId = "22",
                        Price = 20,
                        Alive = true,
                        AcceptedSpeed = 0.5,
                        LimitSpeed = 10
                    }
                };
        }
    }
}