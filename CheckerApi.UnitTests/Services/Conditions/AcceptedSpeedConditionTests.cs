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
    public class AcceptedSpeedConditionTests
    {
        private Mock<IServiceProvider> _serviceProvider;
        private Mock<IMemoryCache> _cache;

        [SetUp]
        public void Setup()
        {
            _serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            _cache = new Mock<IMemoryCache>(MockBehavior.Strict);
            object diff = 0d;
            object network = 0d;
            _cache.Setup(s => s.TryGetValue(Constants.DifficultyKey, out diff)).Returns(false);
            _cache.Setup(s => s.TryGetValue(Constants.BtcBtgPriceKey, out network)).Returns(false);
            _serviceProvider.Setup(x => x.GetService(typeof(IMemoryCache)))
                .Returns(_cache.Object);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Compute_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
        {
            // Arrange
            var complier = new AcceptedSpeedCondition(_serviceProvider.Object);
            var config = new ApiConfiguration()
            {
                AcceptedSpeed = 10
            };

            // Act
            var data = complier.Compute(GetLargeBidSet(id), config, new List<PoolHashrate>());

            // Assert
            Assert.IsFalse(data.Any());
        }

        [Test]
        public void Compute_ShouldReturnBids_WhenMetCondition()
        {
            // Arrange
            var complier = new AcceptedSpeedCondition(_serviceProvider.Object);
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    AcceptedSpeed = 11,
                    Alive = true
                }
            };
            var config = new ApiConfiguration()
            {
                AcceptedSpeed = 10
            };

            // Act
            var data = complier.Compute(orders, config, new List<PoolHashrate>());

            // Assert
            Assert.AreEqual(1, data.Count());
        }

        [Test]
        public void Compute_ShouldReturnBids_WhenMetCondition_Multiple()
        {
            // Arrange
            var complier = new AcceptedSpeedCondition(_serviceProvider.Object);
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    AcceptedSpeed = 15,
                    Alive = true
                },
                new BidEntry()
                {
                    AcceptedSpeed = 1000,
                    Alive = false
                },
                new BidEntry()
                {
                    AcceptedSpeed = 51,
                    Alive = true
                }
            };
            var config = new ApiConfiguration()
            {
                AcceptedSpeed = 10
            };

            // Act
            var data = complier.Compute(orders, config, new List<PoolHashrate>());

            // Assert
            Assert.AreEqual(2, data.Count());
            Assert.IsNotEmpty(data.First().Message);
            Assert.IsNotEmpty(data.First().Condition);
        }

        [Test]
        public void Compute_ShouldNotReturnBids_WhenMetConditionByCached()
        {
            // Arrange
            var complier = new AcceptedSpeedCondition(_serviceProvider.Object);
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    AcceptedSpeed = 12,
                    Alive = true
                }
            };
            var config = new ApiConfiguration()
            {
                AcceptedSpeed = 10
            };

            // Act
            var data = complier.Compute(orders, config, new List<PoolHashrate>());
            var data1 = complier.Compute(orders, config, new List<PoolHashrate>());

            // Assert
            Assert.AreEqual(1, data.Count());
            Assert.IsFalse(data1.Any());
        }

        private IEnumerable<BidEntry> GetLargeBidSet(int id)
        {
            var data = new Dictionary<int, IEnumerable<BidEntry>>()
            {
                {
                    1, new List<BidEntry>()
                    {
                        new BidEntry()
                        {
                            Alive = false,
                            AcceptedSpeed = 1
                        }
                    }
                },
                {
                    2, new List<BidEntry>()
                    {
                        new BidEntry()
                        {
                            Alive = true,
                            AcceptedSpeed = 1
                        },
                        new BidEntry()
                        {
                            Alive = false,
                            AcceptedSpeed = 11
                        }
                    }
                },
                {
                    3, new List<BidEntry>()
                }
            };

            return data[id];
        }
    }
}
