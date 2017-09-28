using System.Net.Http;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class CurrentRequestHandlerFixture
    {
        [Fact]
        public void HandlerSetsHttpRequestMessageToProvider()
        {
            // Arrange
            var request = new HttpRequestMessage();

            // Act
            CurrentRequestHandler.UpdateScopeWithHttpRequestMessage(request);
            var result = AutofacHttpRequestMessageProvider.Current;

            // Assert
            Assert.Equal(request, result);
        }
    }
}
