using System;
using System.Net.Http;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class HttpRequestMessageProviderFixture
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CurrentReturnsHttpRequestMessageWhenSet(bool useAsyncLocal)
        {
            AppContext.SetSwitch("Autofac.Integration.WebApi.HttpRequestMessageProvider.UseAsyncLocal", useAsyncLocal);

            try
            {
                // Arrange
                var httpRequestMessage = new HttpRequestMessage();

                // Act
                HttpRequestMessageProvider.Current = httpRequestMessage;
                var result = HttpRequestMessageProvider.Current;

                // Assert
                Assert.Same(httpRequestMessage, result);
            }
            finally
            {
                AppContext.SetSwitch("Autofac.Integration.WebApi.HttpRequestMessageProvider.UseAsyncLocal", false);
            }
        }

        [Fact]
        public void CurrentReturnsNotSharedWithAsyncLocalAndCallContext()
        {
            AppContext.SetSwitch("Autofac.Integration.WebApi.HttpRequestMessageProvider.UseAsyncLocal", true);

            try
            {
                // Arrange
                var httpRequestMessage = new HttpRequestMessage();

                // Act
                HttpRequestMessageProvider.Current = httpRequestMessage;
            }
            finally
            {
                AppContext.SetSwitch("Autofac.Integration.WebApi.HttpRequestMessageProvider.UseAsyncLocal", false);
            }

            // Assert
            Assert.Null(HttpRequestMessageProvider.Current);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CurrentReturnsNullWhenHttpRequestMessageNotSet(bool useAsyncLocal)
        {
            AppContext.SetSwitch("Autofac.Integration.WebApi.HttpRequestMessageProvider.UseAsyncLocal", useAsyncLocal);

            try
            {
                Assert.Null(HttpRequestMessageProvider.Current);
            }
            finally
            {
                AppContext.SetSwitch("Autofac.Integration.WebApi.HttpRequestMessageProvider.UseAsyncLocal", false);
            }
        }
    }
}
