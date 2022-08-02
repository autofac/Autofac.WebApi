// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.WebApi.Test
{
    public class AutofacWebApiDependencyResolverFixture
    {
        [Fact]
        public void NullContainerThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacWebApiDependencyResolver(null));

            Assert.Equal("container", exception.ParamName);
        }

        [Fact]
        public void NullConfigurationActionThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new AutofacWebApiDependencyResolver(new ContainerBuilder().Build(), null));

            Assert.Equal("configurationAction", exception.ParamName);
        }

        [Fact]
        public void GetServiceReturnsNullForUnregisteredService()
        {
            var container = new ContainerBuilder().Build();
            using var resolver = new AutofacWebApiDependencyResolver(container);

            var service = resolver.GetService(typeof(object));

            Assert.Null(service);
        }

        [Fact]
        public void GetServiceReturnsRegisteredService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var container = builder.Build();
            using var resolver = new AutofacWebApiDependencyResolver(container);

            var service = resolver.GetService(typeof(object));

            Assert.NotNull(service);
        }

        [Fact]
        public void GetServicesReturnsEmptyEnumerableForUnregisteredService()
        {
            var container = new ContainerBuilder().Build();
            using var resolver = new AutofacWebApiDependencyResolver(container);

            var services = resolver.GetServices(typeof(object));

            Assert.Empty(services);
        }

        [Fact]
        public void GetServicesReturnsRegisteredService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var container = builder.Build();
            using var resolver = new AutofacWebApiDependencyResolver(container);

            var services = resolver.GetServices(typeof(object));

            Assert.Single(services);
        }

        [Fact]
        public void GetServicesReturnsRegisteredServices()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            builder.Register(c => new object());
            var container = builder.Build();
            using var resolver = new AutofacWebApiDependencyResolver(container);

            var services = resolver.GetServices(typeof(object));

            Assert.Equal(2, services.Count());
        }

        [Fact]
        public void BeginScopeReturnsNewScopeOnEachCall()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var container = builder.Build();
            using var resolver = new AutofacWebApiDependencyResolver(container);

            Assert.NotSame(resolver.BeginScope(), resolver.BeginScope());
        }

        [Fact]
        public void BeginScopeUsesConfigurationActionIfAny()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object());
            var container = builder.Build();
            using var resolver = new AutofacWebApiDependencyResolver(container, containerBuilder => containerBuilder.Register(c => new object()));
            var services = resolver.GetServices(typeof(object));
            var servicesInScope = resolver.BeginScope().GetServices(typeof(object));

            Assert.NotEqual(services.Count(), servicesInScope.Count());
        }
    }
}
