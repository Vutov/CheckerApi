//using System.Collections.Generic;
//using System.Linq;
//using CheckerApi.Data.Entities;
//using CheckerApi.Services;
//using NUnit.Framework;

//namespace CheckerApi.UnitTests.Services
//{
//    [TestFixture]
//    public class ConditionComplierTests
//    {
//        [TestCase(1)]
//        [TestCase(2)]
//        [TestCase(3)]
//        public void AcceptedSpeedCondition_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var config = new ApiConfiguration()
//            {
//                AcceptedSpeed = 10
//            };

//            // Act
//            var data = complier.AcceptedSpeedCondition(GetLargeBidSet(id), config);

//            // Assert
//            Assert.IsFalse(data.Any());
//        }

//        [Test]
//        public void AcceptedSpeedCondition_ShouldReturnBids_WhenMetCondition()
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var orders = new List<BidEntry>()
//            {
//                new BidEntry()
//                {
//                    AcceptedSpeed = 11,
//                    Alive = true
//                }
//            };
//            var config = new ApiConfiguration()
//            {
//                AcceptedSpeed = 10
//            };

//            // Act
//            var data = complier.AcceptedSpeedCondition(orders, config);

//            // Assert
//            Assert.AreEqual(1, data.Count());
//        }

//        [Test]
//        public void AcceptedSpeedCondition_ShouldReturnBids_WhenMetCondition_Multiple()
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var orders = new List<BidEntry>()
//            {
//                new BidEntry()
//                {
//                    AcceptedSpeed = 15,
//                    Alive = true
//                },
//                new BidEntry()
//                {
//                    AcceptedSpeed = 1000,
//                    Alive = false
//                },
//                new BidEntry()
//                {
//                    AcceptedSpeed = 51,
//                    Alive = true
//                }
//            };
//            var config = new ApiConfiguration()
//            {
//                AcceptedSpeed = 10
//            };

//            // Act
//            var data = complier.AcceptedSpeedCondition(orders, config);

//            // Assert
//            Assert.AreEqual(2, data.Count());
//            Assert.IsNotEmpty(data.First().Message);
//            Assert.IsNotEmpty(data.First().Condition);
//        }

//        [Test]
//        public void AcceptedSpeedCondition_ShouldNotReturnBids_WhenMetConditionByCached()
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var orders = new List<BidEntry>()
//            {
//                new BidEntry()
//                {
//                    AcceptedSpeed = 12,
//                    Alive = true
//                }
//            };
//            var config = new ApiConfiguration()
//            {
//                AcceptedSpeed = 10
//            };

//            // Act
//            var data = complier.AcceptedSpeedCondition(orders, config);
//            var data1 = complier.AcceptedSpeedCondition(orders, config);

//            // Assert
//            Assert.AreEqual(1, data.Count());
//            Assert.IsFalse(data1.Any());
//        }

//        [TestCase(1)]
//        [TestCase(2)]
//        [TestCase(3)]
//        public void SignOfAttack_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var config = new ApiConfiguration()
//            {
//                PriceThreshold = 1,
//                LimitSpeed = 10
//            };

//            // Act
//            var data = complier.SignOfAttack(GetSignBidSet(id), config);

//            // Assert
//            Assert.IsFalse(data.Any());
//        }

//        [Test]
//        public void SignOfAttack_ShouldReturnBids_WhenMetCondition_ZeroLimit()
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var orders = new List<BidEntry>()
//            {
//                new BidEntry()
//                {
//                    Price = 1,
//                    Alive = true,
//                    AcceptedSpeed = 1,
//                    LimitSpeed = 4
//                },
//                new BidEntry()
//                {
//                    Price = 2,
//                    Alive = true,
//                    LimitSpeed = 0
//                }
//            };
//            var config = new ApiConfiguration()
//            {
//                PriceThreshold = 1,
//                LimitSpeed = 10
//            };

//            // Act
//            var data = complier.SignOfAttack(orders, config);

//            // Assert
//            Assert.AreEqual(1, data.Count());
//        }

//        [Test]
//        public void SignOfAttack_ShouldReturnBids_WhenMetCondition_HasLimit()
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var orders = new List<BidEntry>()
//            {
//                new BidEntry()
//                {
//                    NiceHashId = "15",
//                    Price = 1,
//                    Alive = true,
//                    AcceptedSpeed = 1,
//                    LimitSpeed = 1
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "16",
//                    Price = 2,
//                    Alive = true,
//                    LimitSpeed = 10
//                }
//            };
//            var config = new ApiConfiguration()
//            {
//                PriceThreshold = 1,
//                LimitSpeed = 10
//            };

//            // Act
//            var data = complier.SignOfAttack(orders, config);

//            // Assert
//            Assert.AreEqual(1, data.Count());
//            Assert.IsNotEmpty(data.First().Message);
//            Assert.IsNotEmpty(data.First().Condition);
//        }

//        [Test]
//        public void SignOfAttack_ShouldReturnBidsUpdate_WhenBidSeen()
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var orders = new List<BidEntry>()
//            {
//                new BidEntry()
//                {
//                    NiceHashId = "11",
//                    Price = 11,
//                    Alive = true,
//                    AcceptedSpeed = 1
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "11",
//                    Price = 22,
//                    Alive = true,
//                    LimitSpeed = 0
//                }
//            };
//            var config = new ApiConfiguration()
//            {
//                PriceThreshold = 1,
//                LimitSpeed = 10
//            };

//            // Act
//            var data = complier.SignOfAttack(orders, config);
//            var data1 = complier.SignOfAttack(orders, config);

//            // Assert
//            Assert.AreEqual(1, data.Count());
//            Assert.AreEqual(1, data1.Count());
//            Assert.IsTrue(data1.First().Message.Contains("Progress"));
//        }

//        [Test]
//        public void SignOfAttack_ShouldReturnBids_WhenWithinThreashold()
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var orders = new List<BidEntry>()
//            {
//                new BidEntry()
//                {
//                    NiceHashId = "21",
//                    Price = 21.5,
//                    Alive = true,
//                    AcceptedSpeed = 1
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "20",
//                    Price = 22,
//                    Alive = true,
//                    LimitSpeed = 10
//                }
//            };
//            var config = new ApiConfiguration()
//            {
//                PriceThreshold = 1,
//                LimitSpeed = 11
//            };

//            // Act
//            var data = complier.SignOfAttack(orders, config);

//            // Assert
//            Assert.AreEqual(1, data.Count());
//            Assert.AreEqual("21", data.First().BidEntry.NiceHashId);
//        }

//        [TestCase(1)]
//        [TestCase(2)]
//        [TestCase(3)]
//        public void PercentThresholdAttack_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
//        {
//            // Arrange
//            var complier = new ConditionComplier();
//            var config = new ApiConfiguration()
//            {
//                LimitSpeed = 10
//            }; ;

//            // Act
//            var data = complier.PercentThresholdAttack(GetSignBidSet(id), config);

//            // Assert
//            Assert.IsFalse(data.Any());
//        }

        
//        [Test]
//        public void PercentThresholdAttack_ShouldReturnBids_WhenMetCondition()
//        {
//            // Arrange
//            var complier = new ConditionComplier();

//            // Total AcceptedSpeed = 45, 10% = 4.5, order by price - ID 22, 19, 18. 4,5 power from top is id 19.
//            // price threahold above is 10. Orders with price > 10 and matching conditions are id 22, 19
//            var orders = GetPercentThresholdAttackMetConditionBids();

//            var config = new ApiConfiguration()
//            {
//                LimitSpeed = 10
//            };

//            // Act
//            var data = complier.PercentThresholdAttack(orders, config).OrderBy(o => o.BidEntry.NiceHashId).ToList();

//            // Asserts
//            Assert.AreEqual(2, data.Count());
//            Assert.IsNotEmpty("19", data[0].BidEntry.NiceHashId);
//            Assert.IsNotEmpty("22", data[1].BidEntry.NiceHashId);
//        }

//        [Test]
//        public void PercentThresholdAttack_ShouldReturnBidsUpdate_WhenBidSeen()
//        {
//            // Arrange
//            var complier = new ConditionComplier();

//            // Total AcceptedSpeed = 45, 10% = 4.5, order by price - ID 22, 19, 18. 4,5 power from top is id 19.
//            // price threahold above is 10. Orders with price > 10 and matching conditions are id 22, 19
//            var orders = GetPercentThresholdAttackMetConditionBids();

//            var config = new ApiConfiguration()
//            {
//                LimitSpeed = 10
//            };

//            // Act
//            var data = complier.PercentThresholdAttack(orders, config);
//            var data1 = complier.PercentThresholdAttack(orders, config);

//            // Assert
//            Assert.AreEqual(2, data.Count());
//            Assert.AreEqual(2, data1.Count());
//            Assert.IsTrue(data1.First().Message.Contains("Progress"));
//        }
        
//        private IEnumerable<BidEntry> GetLargeBidSet(int id)
//        {
//            var data = new Dictionary<int, IEnumerable<BidEntry>>()
//            {
//                {
//                    1, new List<BidEntry>()
//                    {
//                        new BidEntry()
//                        {
//                            Alive = false,
//                            AcceptedSpeed = 1
//                        }
//                    }
//                },
//                {
//                    2, new List<BidEntry>()
//                    {
//                        new BidEntry()
//                        {
//                            Alive = true,
//                            AcceptedSpeed = 1
//                        },
//                        new BidEntry()
//                        {
//                            Alive = false,
//                            AcceptedSpeed = 11
//                        }
//                    }
//                },
//                {
//                    3, new List<BidEntry>()
//                }
//            };

//            return data[id];
//        }

//        private IEnumerable<BidEntry> GetSignBidSet(int id)
//        {
//            var data = new Dictionary<int, IEnumerable<BidEntry>>()
//            {
//                {
//                    1, new List<BidEntry>()
//                    {
//                        new BidEntry()
//                        {
//                            NiceHashId = "1",
//                            Alive = true,
//                            LimitSpeed = 5,
//                            Price = 12
//                        }
//                    }
//                },
//                {
//                    2, new List<BidEntry>()
//                    {
//                        new BidEntry()
//                        {
//                            NiceHashId = "3",
//                            Alive = true,
//                            LimitSpeed = 5,
//                            Price = 5
//                        },
//                        new BidEntry()
//                        {
//                            NiceHashId = "4",
//                            Alive = true,
//                            LimitSpeed = 0,
//                            Price = 1.1
//                        }
//                    }
//                },
//                {
//                    3, new List<BidEntry>()
//                }
//            };

//            return data[id];
//        }

//        private IEnumerable<BidEntry> GetPercentThresholdAttackMetConditionBids()
//        {
//            return new List<BidEntry>()
//            {
//                new BidEntry()
//                {
//                    NiceHashId = "15",
//                    Price = 1,
//                    Alive = true,
//                    AcceptedSpeed = 9,
//                    LimitSpeed = 1
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "16",
//                    Price = 2,
//                    Alive = true,
//                    AcceptedSpeed = 1,
//                    LimitSpeed = 0
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "17",
//                    Price = 4,
//                    Alive = true,
//                    AcceptedSpeed = 10,
//                    LimitSpeed = 14
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "18",
//                    Price = 5,
//                    Alive = true,
//                    AcceptedSpeed = 12,
//                    LimitSpeed = 18
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "19",
//                    Price = 10,
//                    Alive = true,
//                    AcceptedSpeed = 13,
//                    LimitSpeed = 0
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "25",
//                    Price = 11,
//                    Alive = true,
//                    AcceptedSpeed = 0.2,
//                    LimitSpeed = 5
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "20",
//                    Price = 2,
//                    Alive = true,
//                    AcceptedSpeed = 0.1,
//                    LimitSpeed = 10
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "21",
//                    Price = 2,
//                    Alive = false,
//                    AcceptedSpeed = 140,
//                    LimitSpeed = 1
//                },
//                new BidEntry()
//                {
//                    NiceHashId = "22",
//                    Price = 20,
//                    Alive = true,
//                    AcceptedSpeed = 0.5,
//                    LimitSpeed = 10
//                }
//            };
//        }

//    }
//}
