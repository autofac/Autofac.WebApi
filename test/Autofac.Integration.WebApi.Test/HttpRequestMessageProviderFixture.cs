using System.Net.Http;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class HttpRequestMessageProviderFixture
    {
        [Fact]
        public void CurrentReturnsHttpRequestMessageWhenSet()
        {
            // Arrange
            var httpRequestMessage = new HttpRequestMessage();

            // Act
            HttpRequestMessageProvider.Current = httpRequestMessage;
            var result = HttpRequestMessageProvider.Current;

            // Assert
            Assert.Same(httpRequestMessage, result);
        }

        [Fact]
        public void CurrentReturnsNullWhenHttpRequestMessageNotSet()
        {
            Assert.Null(HttpRequestMessageProvider.Current);
        }
    }
}
