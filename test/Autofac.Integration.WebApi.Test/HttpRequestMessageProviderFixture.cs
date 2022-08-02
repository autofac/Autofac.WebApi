// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
