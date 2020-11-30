﻿// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac.Core.Lifetime;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class HttpConfigurationExtensionsFixture
    {
        [Fact]
        public void RegisterHttpRequestMessageAddsHandler()
        {
            var config = new HttpConfiguration();

            config.RegisterHttpRequestMessage(new ContainerBuilder());

            Assert.Single(config.MessageHandlers.OfType<CurrentRequestHandler>());
        }

        [Fact]
        public void RegisterHttpRequestMessageEnsuresHandlerAddedOnlyOnce()
        {
            var config = new HttpConfiguration();
            var builder = new ContainerBuilder();

            config.RegisterHttpRequestMessage(builder);
            config.RegisterHttpRequestMessage(builder);

            Assert.Single(config.MessageHandlers.OfType<CurrentRequestHandler>());
        }

        [Fact]
        public void RegisterHttpRequestMessageAddsRegistration()
        {
            var config = new HttpConfiguration();
            var builder = new ContainerBuilder();

            config.RegisterHttpRequestMessage(builder);

            var container = builder.Build();
            Assert.True(container.IsRegistered<HttpRequestMessage>());
        }

        [Fact]
        public async Task RegisterHttpRequestMessageNotDisposeAfterScopeDipose()
        {
            var config = new HttpConfiguration();
            var builder = new ContainerBuilder();

            config.RegisterHttpRequestMessage(builder);

            var container = builder.Build();
            Assert.True(container.IsRegistered<HttpRequestMessage>());

            var httpRequestMessage = new HttpRequestMessage
            {
                Content = new StringContent("")
            };

            HttpRequestMessageProvider.Current = httpRequestMessage;
            var result = HttpRequestMessageProvider.Current;


            using (var scope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                Assert.Same(result, scope.Resolve<HttpRequestMessage>());
            }

            _ = await result.Content.ReadAsStringAsync();
        }
    }
}
