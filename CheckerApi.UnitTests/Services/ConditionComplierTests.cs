using System;
using System.Collections.Generic;
using System.Linq;
using CheckerApi.Data.Entities;
using CheckerApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using NUnit.Framework;

namespace CheckerApi.UnitTests.Services
{
    [TestFixture]
    public class ConditionComplierTests
    {
        private Mock<ILogger<ConditionComplier>> _loggerMock;

        [SetUp]
        public void SetUp()
        {
            this._loggerMock = new Mock<ILogger<ConditionComplier>>(MockBehavior.Strict);
            this._loggerMock.Setup(s => s.Log(It.IsAny<LogLevel>(), 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void AcceptedSpeedCondition_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
        {
            // Arrange
            var complier = new ConditionComplier(_loggerMock.Object);
            var config = new ApiConfiguration()
            {
                AcceptedSpeed = 10
            };

            // Act
            var data = complier.AcceptedSpeedCondition(this.GetLargeBidSet(id), config);

            // Assert
            Assert.IsFalse(data.bids.Any());
        }

        [Test]
        public void AcceptedSpeedCondition_ShouldReturnBids_WhenMetCondition()
        {
            // Arrange
            var complier = new ConditionComplier(_loggerMock.Object);
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
            Assert.AreEqual(1, data.bids.Count());
        }

        [Test]
        public void AcceptedSpeedCondition_ShouldReturnBids_WhenMetCondition_Multiple()
        {
            // Arrange
            var complier = new ConditionComplier(_loggerMock.Object);
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
            Assert.AreEqual(2, data.bids.Count());
            Assert.IsNotEmpty(data.message);
            Assert.IsNotEmpty(data.condition);
        }

        [Test]
        public void AcceptedSpeedCondition_ShouldNotReturnBids_WhenMetConditionByCached()
        {
            // Arrange
            var complier = new ConditionComplier(_loggerMock.Object);
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
            Assert.AreEqual(1, data.bids.Count());
            Assert.IsFalse(data1.bids.Any());
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void SignOfAttack_ShouldReturnEmptyList_WhenNothingMeetsCondition(int id)
        {
            // Arrange
            var complier = new ConditionComplier(_loggerMock.Object);
            var config = new ApiConfiguration()
            {
                PriceThreshold = 1,
                LimitSpeed = 10
            };

            // Act
            var data = complier.SignOfAttack(this.GetSignBidSet(id), config);

            // Assert
            Assert.IsFalse(data.bids.Any());
        }

        [Test]
        public void SignOfAttack_ShouldReturnBids_WhenMetCondition_ZeroLimit()
        {
            // Arrange
            var complier = new ConditionComplier(_loggerMock.Object);
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
            Assert.AreEqual(1, data.bids.Count());
        }

        [Test]
        public void SignOfAttack_ShouldReturnBids_WhenMetCondition_HasLimit()
        {
            // Arrange
            var complier = new ConditionComplier(_loggerMock.Object);
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
            Assert.AreEqual(1, data.bids.Count());
            Assert.IsNotEmpty(data.message);
            Assert.IsNotEmpty(data.condition);
        }

        [Test]
        public void SignOfAttack_ShouldNotReturnBids_WhenMetConditionByCached()
        {
            // Arrange
            var complier = new ConditionComplier(_loggerMock.Object);
            var orders = new List<BidEntry>()
            {
                new BidEntry()
                {
                    Price = 11,
                    Alive = true,
                    AcceptedSpeed = 1
                },
                new BidEntry()
                {
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
            Assert.AreEqual(1, data.bids.Count());
            Assert.IsFalse(data1.bids.Any());
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
                            Alive = true,
                            LimitSpeed = 0,
                            Price = 12
                        },
                        new BidEntry()
                        {
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
                            Alive = true,
                            LimitSpeed = 0,
                            Price = 1
                        },
                        new BidEntry()
                        {
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
