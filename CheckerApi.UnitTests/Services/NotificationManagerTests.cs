using System;
using CheckerApi.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Moq;
using NUnit.Framework;
using RestSharp;

namespace CheckerApi.UnitTests.Services
{
    [TestFixture]
    public class NotificationManagerTests
    {
        private NotificationManager sut;
        private Mock<IConfiguration> _configMock;
        private Mock<ILogger<NotificationManager>> _loggerMock;
        private Mock<IRestClient> _restMock;

        [SetUp]
        public void SetUp()
        {
            _configMock = new Mock<IConfiguration>(MockBehavior.Strict);
            var domainMock = new Mock<IConfigurationSection>();
            domainMock.SetupGet(s => s.Value).Returns("http://some.some");
            _configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(domainMock.Object);
            _loggerMock = new Mock<ILogger<NotificationManager>>(MockBehavior.Strict);
            _loggerMock.Setup(s => s.Log(It.IsAny<LogLevel>(), 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();
            _restMock = new Mock<IRestClient>(MockBehavior.Strict);
            _restMock.SetupSet<Uri>(s => s.BaseUrl = It.IsAny<Uri>());
            sut = new NotificationManager(_restMock.Object, _configMock.Object, _loggerMock.Object);
        }

        [Test]
        public void TriggerHook_ShouldReturnFail_WhenException()
        {
            // Arrange
            _restMock.Setup(r => r.Execute(It.IsAny<RestRequest>())).Throws(new InvalidOperationException());

            // Act
            var res = sut.TriggerHook();

            // Assert
            Assert.IsTrue(res.HasFailed());
            _loggerMock.Verify(s => s.Log(LogLevel.Critical, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Test]
        public void TriggerHook_ShouldReturnOk_WhenTriggerSent()
        {
            // Arrange
            _restMock.Setup(r => r.Execute(It.IsAny<RestRequest>())).Returns((IRestResponse)null);

            // Act
            var res = sut.TriggerHook();

            // Assert
            Assert.IsTrue(res.IsSuccess());
        }

        [Test]
        public void TriggerHook_ShouldAddMessages_WhenTriggeringSent()
        {
            // Arrange
            RestRequest givenRequest = null;
            _restMock.Setup(r => r.Execute(It.IsAny<RestRequest>())).Returns((IRestResponse)null).Callback<RestRequest>(r => givenRequest = r);

            // Act
            var res = sut.TriggerHook("One", "Two");

            // Assert
            Assert.IsTrue(res.IsSuccess());
            Assert.AreEqual(2, givenRequest.Parameters.Count);
        }
    }
}
