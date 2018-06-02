using System.Collections.Generic;
using System.Linq;
using CheckerApi.Data.Entities;
using CheckerApi.Services;
using NUnit.Framework;

namespace CheckerApi.UnitTests.Services
{
    [TestFixture]
    public class ConditionComplierTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void AcceptedSpeedCondition_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
        {
            // Arrange
            var complier = new ConditionComplier();
            var config = new ApiConfiguration()
            {
                AcceptedSpeed = 10
            };

            // Act
            var data = complier.AcceptedSpeedCondition(this.GetLargeBidSet(id), config);

            // Assert
            Assert.IsFalse(data.Any());
        }

        [Test]
        public void AcceptedSpeedCondition_ShouldReturnBids_WhenMetCondition()
        {
            // Arrange
            var complier = new ConditionComplier();
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
            var data = complier.AcceptedSpeedCondition(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
        }

        [Test]
        public void AcceptedSpeedCondition_ShouldReturnBids_WhenMetCondition_Multiple()
        {
            // Arrange
            var complier = new ConditionComplier();
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
            var data = complier.AcceptedSpeedCondition(orders, config);

            // Assert
            Assert.AreEqual(2, data.Count());
            Assert.IsNotEmpty(data.First().Message);
            Assert.IsNotEmpty(data.First().Condition);
        }

        [Test]
        public void AcceptedSpeedCondition_ShouldNotReturnBids_WhenMetConditionByCached()
        {
            // Arrange
            var complier = new ConditionComplier();
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
            var data = complier.AcceptedSpeedCondition(orders, config);
            var data1 = complier.AcceptedSpeedCondition(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
            Assert.IsFalse(data1.Any());
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void SignOfAttack_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
        {
            // Arrange
            var complier = new ConditionComplier();
            var config = new ApiConfiguration()
            {
                PriceThreshold = 1,
                LimitSpeed = 10
            };

            // Act
            var data = complier.SignOfAttack(this.GetSignBidSet(id), config);

            // Assert
            Assert.IsFalse(data.Any());
        }

        [Test]
        public void SignOfAttack_ShouldReturnBids_WhenMetCondition_ZeroLimit()
        {
            // Arrange
            var complier = new ConditionComplier();
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    Price = 1,
                    Alive = true,
                    AcceptedSpeed = 1
                },
                new BidEntry()
                {
                    Price = 2,
                    Alive = true,
                    LimitSpeed = 0
                }
            };
            var config = new ApiConfiguration()
            {
                PriceThreshold = 1,
                LimitSpeed = 10
            };

            // Act
            var data = complier.SignOfAttack(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
        }

        [Test]
        public void SignOfAttack_ShouldReturnBids_WhenMetCondition_HasLimit()
        {
            // Arrange
            var complier = new ConditionComplier();
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    NiceHashId = "15",
                    Price = 1,
                    Alive = true,
                    AcceptedSpeed = 1
                },
                new BidEntry()
                {
                    NiceHashId = "16",
                    Price = 2,
                    Alive = true,
                    LimitSpeed = 10
                }
            };
            var config = new ApiConfiguration()
            {
                PriceThreshold = 1,
                LimitSpeed = 10
            };

            // Act
            var data = complier.SignOfAttack(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
            Assert.IsNotEmpty(data.First().Message);
            Assert.IsNotEmpty(data.First().Condition);
        }

        [Test]
        public void SignOfAttack_ShouldReturnBidsUpdate_WhenBidSeen()
        {
            // Arrange
            var complier = new ConditionComplier();
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    NiceHashId = "11",
                    Price = 11,
                    Alive = true,
                    AcceptedSpeed = 1
                },
                new BidEntry()
                {
                    NiceHashId = "11",
                    Price = 22,
                    Alive = true,
                    LimitSpeed = 0
                }
            };
            var config = new ApiConfiguration()
            {
                PriceThreshold = 1,
                LimitSpeed = 10
            };

            // Act
            var data = complier.SignOfAttack(orders, config);
            var data1 = complier.SignOfAttack(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
            Assert.AreEqual(1, data1.Count());
            Assert.IsTrue(data1.First().Message.Contains("Progress"));
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
                            LimitSpeed = 0,
                            Price = 12
                        },
                        new BidEntry()
                        {
                            NiceHashId = "2",
                            Alive = true,
                            LimitSpeed = 0,
                            Price = 12
                        }
                    }
                },
                {
                    2, new List<BidEntry>()
                    {
                        new BidEntry()
                        {
                            NiceHashId = "1",
                            Alive = true,
                            LimitSpeed = 0,
                            Price = 1
                        },
                        new BidEntry()
                        {
                            NiceHashId = "1",
                            Alive = true,
                            LimitSpeed = 0,
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
    }
}
