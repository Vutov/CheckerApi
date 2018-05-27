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
            this._configMock = new Mock<IConfiguration>(MockBehavior.Strict);
            var domainMock = new Mock<IConfigurationSection>();
            domainMock.SetupGet(s => s.Value).Returns("http://some.some");
            this._configMock.Setup(c => c.GetSection(It.IsAny<string>())).Returns(domainMock.Object);
            this._loggerMock = new Mock<ILogger<NotificationManager>>(MockBehavior.Strict);
            this._loggerMock.Setup(s => s.Log(It.IsAny<LogLevel>(), 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();
            this._restMock = new Mock<IRestClient>(MockBehavior.Strict);
            this._restMock.SetupSet<Uri>(s => s.BaseUrl = It.IsAny<Uri>());
            this.sut = new NotificationManager(_restMock.Object, _configMock.Object, _loggerMock.Object);
        }

        [Test]
        public void TriggerHook_ShouldReturnFail_WhenException()
        {
            // Arrange
            this._restMock.Setup(r => r.Execute(It.IsAny<RestRequest>())).Throws(new InvalidOperationException());

            // Act
            var res = this.sut.TriggerHook();

            // Assert
            Assert.IsTrue(res.HasFailed());
            this._loggerMock.Verify(s => s.Log(LogLevel.Critical, 0, It.IsAny<FormattedLogValues>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }

        [Test]
        public void TriggerHook_ShouldReturnOk_WhenTriggerSent()
        {
            // Arrange
            this._restMock.Setup(r => r.Execute(It.IsAny<RestRequest>())).Returns((IRestResponse)null);

            // Act
            var res = this.sut.TriggerHook();

            // Assert
            Assert.IsTrue(res.IsSuccess());
        }

        [Test]
        public void TriggerHook_ShouldAddMessages_WhenTriggeringSent()
        {
            // Arrange
            RestRequest givenRequest = null;
            this._restMock.Setup(r => r.Execute(It.IsAny<RestRequest>())).Returns((IRestResponse)null).Callback<RestRequest>(r => givenRequest = r);

            // Act
            var res = this.sut.TriggerHook("One", "Two");

            // Assert
            Assert.IsTrue(res.IsSuccess());
            Assert.AreEqual(2, givenRequest.Parameters.Count);
        }
    }
}
