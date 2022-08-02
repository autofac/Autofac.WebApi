// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core.Lifetime;

namespace Autofac.Integration.WebApi.Test;

public class AutofacWebApiDependencyScopeFixture
{
    [Fact]
    public void NullContainerThrowsException()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new AutofacWebApiDependencyScope(null));

        Assert.Equal("lifetimeScope", exception.ParamName);
    }

    [Fact]
    public void GetServiceReturnsNullForUnregisteredService()
    {
        var lifetimeScope = new ContainerBuilder().Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        using var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);

        var service = dependencyScope.GetService(typeof(object));

        Assert.Null(service);
    }

    [Fact]
    public void GetServiceReturnsRegisteredService()
    {
        var builder = new ContainerBuilder();
        builder.Register(c => new object()).InstancePerRequest();
        var lifetimeScope = builder.Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        using var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);

        var service = dependencyScope.GetService(typeof(object));

        Assert.NotNull(service);
    }

    [Fact]
    public void GetServicesReturnsRegisteredServices()
    {
        var builder = new ContainerBuilder();
        builder.Register(c => new object()).InstancePerRequest();
        builder.Register(c => new object()).InstancePerRequest();
        var lifetimeScope = builder.Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        using var resolver = new AutofacWebApiDependencyScope(lifetimeScope);

        var services = resolver.GetServices(typeof(object));

        Assert.Equal(2, services.Count());
    }

    [Fact]
    public void GetServicesReturnsEmptyEnumerableForUnregisteredService()
    {
        var lifetimeScope = new ContainerBuilder().Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        using var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);

        var services = dependencyScope.GetServices(typeof(object));

        Assert.Empty(services);
    }

    [Fact]
    public void GetServicesReturnsRegisteredService()
    {
        var builder = new ContainerBuilder();
        builder.Register(c => new object()).InstancePerRequest();
        var lifetimeScope = builder.Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
        using var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);

        var services = dependencyScope.GetServices(typeof(object));

        Assert.Single(services);
    }
}
