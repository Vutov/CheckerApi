using System.Collections.Generic;
using System.Linq;
using CheckerApi.Models.Entities;
using CheckerApi.Services.Conditions;
using NUnit.Framework;

namespace CheckerApi.UnitTests.Services.Conditions
{
    [TestFixture]
    public class PercentThresholdConditionTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Compute_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
        {
            // Arrange
            var complier = new PercentThresholdCondition();
            var config = new ApiConfiguration()
            {
                LimitSpeed = 10
            }; ;

            // Act
            var data = complier.Compute(GetSignBidSet(id), config);

            // Assert
            Assert.IsFalse(data.Any());
        }


        [Test]
        public void Compute_ShouldReturnBids_WhenMetCondition()
        {
            // Arrange
            var complier = new PercentThresholdCondition();

            // Total AcceptedSpeed = 45, 10% = 4.5, order by price - ID 22, 19, 18. 4,5 power from top is id 19.
            // price threahold above is 10. Orders with price > 10 and matching conditions are id 22, 19
            var orders = GetPercentThresholdAttackMetConditionBids();

            var config = new ApiConfiguration()
            {
                LimitSpeed = 10,
                MinimalAcceptedSpeed = 0,
                AcceptedPercentThreshold = 0.10 // 10%
            };

            // Act
            var data = complier.Compute(orders, config).OrderBy(o => o.BidEntry.NiceHashId).ToList();

            // Asserts
            Assert.AreEqual(2, data.Count());
            Assert.IsNotEmpty("19", data[0].BidEntry.NiceHashId);
            Assert.IsNotEmpty("22", data[1].BidEntry.NiceHashId);
        }

        [Test]
        public void Compute_ShouldReturnBidsUpdate_WhenBidSeen()
        {
            // Arrange
            var complier = new PercentThresholdCondition();

            // Total AcceptedSpeed = 45, 10% = 4.5, order by price - ID 22, 19, 18. 4,5 power from top is id 19.
            // price threahold above is 10. Orders with price > 10 and matching conditions are id 22, 19
            // accepted speed limit 1 - only 19
            var orders = GetPercentThresholdAttackMetConditionBids();

            var config = new ApiConfiguration()
            {
                LimitSpeed = 10,
                MinimalAcceptedSpeed = 1,
                AcceptedPercentThreshold = 0.10 // 10%
            };

            // Act
            var data = complier.Compute(orders, config);
            var data1 = complier.Compute(orders, config);

            // Assert
            Assert.AreEqual(1, data.Count());
            Assert.AreEqual(1, data1.Count());
            Assert.AreEqual("19", data1.First().BidEntry.NiceHashId);
            Assert.IsTrue(data1.First().Message.Contains("Progress"));
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

        private IEnumerable<BidEntry> GetPercentThresholdAttackMetConditionBids()
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
