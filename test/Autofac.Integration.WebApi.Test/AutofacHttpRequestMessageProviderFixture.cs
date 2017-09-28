using System.Net.Http;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class AutofacHttpRequestMessageProviderFixture
    {
        [Fact]
        public void SetCurrentReturnsExpectedHttpRequestMessageInGet()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage();

            // Act
            AutofacHttpRequestMessageProvider.Current = httpRequestMessage;
            var result = AutofacHttpRequestMessageProvider.Current;

            // Assert
            Assert.Same(httpRequestMessage, result);
        }
    }
}
