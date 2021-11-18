// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class CurrentRequestHandlerFixture
    {
        [Fact]
        public void HandlerSetsHttpRequestMessageOnProvider()
        {
            // Arrange
            var request = new HttpRequestMessage();

            // Act
            CurrentRequestHandler.UpdateScopeWithHttpRequestMessage(request);
            var result = HttpRequestMessageProvider.Current;

            // Assert
            Assert.Equal(request, result);
        }
    }
}
