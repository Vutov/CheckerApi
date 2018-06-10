using System.Collections.Generic;
using System.Linq;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Conditions;
using NUnit.Framework;

namespace CheckerApi.UnitTests.Services.Conditions
{
    [TestFixture]
    public class SignOfAttackConditionTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Compute_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
        {
            // Arrange
            var complier = new SignOfAttackCondition();
            var config = new ApiConfiguration()
            {
                PriceThreshold = 1,
                LimitSpeed = 10
            };

            // Act
            var data = complier.Compute(GetSignBidSet(id), config);

            // Assert
            Assert.IsFalse(data.Any());
        }

        [Test]
        public void Compute_ShouldReturnBids_WhenMetCondition_ZeroLimit()
        {
            // Arrange
            var complier = new SignOfAttackCondition();
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    Price = 1,
                    Alive = true,
                    AcceptedSpeed = 1,
                    LimitSpeed = 4
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
            var data = complier.Compute(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
        }

        [Test]
        public void Compute_ShouldReturnBids_WhenBelowMinimalAcceptedSpeed_ZeroLimit()
        {
            // Arrange
            var complier = new SignOfAttackCondition();
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    Price = 1,
                    Alive = true,
                    AcceptedSpeed = 1,
                    LimitSpeed = 4
                },
                new BidEntry()
                {
                    Price = 2,
                    Alive = true,
                    LimitSpeed = 0,
                    AcceptedSpeed = 2
                }
            };
            var config = new ApiConfiguration()
            {
                PriceThreshold = 1,
                LimitSpeed = 10,
                MinimalAcceptedSpeed = 4
            };

            // Act
            var data = complier.Compute(orders, config);

            // Assert
            Assert.AreEqual(0, data.Count());
        }

        [Test]
        public void Compute_ShouldReturnBids_WhenMetCondition_HasLimit()
        {
            // Arrange
            var complier = new SignOfAttackCondition();
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    NiceHashId = "15",
                    Price = 1,
                    Alive = true,
                    AcceptedSpeed = 1,
                    LimitSpeed = 1
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
            var data = complier.Compute(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
            Assert.IsNotEmpty(data.First().Message);
            Assert.IsNotEmpty(data.First().Condition);
        }

        [Test]
        public void Compute_ShouldReturnBidsUpdate_WhenBidSeen()
        {
            // Arrange
            var complier = new SignOfAttackCondition();
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
            var data = complier.Compute(orders, config);
            var data1 = complier.Compute(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
            Assert.AreEqual(1, data1.Count());
            Assert.IsTrue(data1.First().Message.Contains("Progress"));
        }

        [Test]
        public void Compute_ShouldReturnBids_WhenWithinThreashold()
        {
            // Arrange
            var complier = new SignOfAttackCondition();
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    NiceHashId = "21",
                    Price = 21.5,
                    Alive = true,
                    AcceptedSpeed = 1
                },
                new BidEntry()
                {
                    NiceHashId = "20",
                    Price = 22,
                    Alive = true,
                    LimitSpeed = 10
                }
            };
            var config = new ApiConfiguration()
            {
                PriceThreshold = 1,
                LimitSpeed = 11
            };

            // Act
            var data = complier.Compute(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
            Assert.AreEqual("21", data.First().BidEntry.NiceHashId);
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
                            LimitSpeed = 5,
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
                            LimitSpeed = 5,
                            Price = 5
                        },
                        new BidEntry()
                        {
                            NiceHashId = "4",
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