// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http;
using Autofac.Core.Lifetime;

namespace Autofac.Integration.WebApi.Test;

public class HttpConfigurationExtensionsFixture
{
    [Fact]
    public void RegisterHttpRequestMessageAddsHandler()
    {
        using var config = new HttpConfiguration();

        config.RegisterHttpRequestMessage(new ContainerBuilder());

        Assert.Single(config.MessageHandlers.OfType<CurrentRequestHandler>());
    }

    [Fact]
    public void RegisterHttpRequestMessageEnsuresHandlerAddedOnlyOnce()
    {
        using var config = new HttpConfiguration();
        var builder = new ContainerBuilder();

        config.RegisterHttpRequestMessage(builder);
        config.RegisterHttpRequestMessage(builder);

        Assert.Single(config.MessageHandlers.OfType<CurrentRequestHandler>());
    }

    [Fact]
    public void RegisterHttpRequestMessageAddsRegistration()
    {
        using var config = new HttpConfiguration();
        var builder = new ContainerBuilder();

        config.RegisterHttpRequestMessage(builder);

        var container = builder.Build();
        Assert.True(container.IsRegistered<HttpRequestMessage>());
    }

    [Fact]
    public async Task RegisterHttpRequestMessageNotDisposeAfterScopeDipose()
    {
        using var config = new HttpConfiguration();
        var builder = new ContainerBuilder();

        config.RegisterHttpRequestMessage(builder);

        var container = builder.Build();
        Assert.True(container.IsRegistered<HttpRequestMessage>());

        var httpRequestMessage = new HttpRequestMessage
        {
            Content = new StringContent(""),
        };

        HttpRequestMessageProvider.Current = httpRequestMessage;
        var result = HttpRequestMessageProvider.Current;

        using (var scope = container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
        {
            Assert.Same(result, scope.Resolve<HttpRequestMessage>());
        }

        _ = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
    }
}
