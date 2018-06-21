// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
// http://autofac.org
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

using System;
using System.Linq;
using Autofac.Core.Lifetime;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
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
            var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);

            var service = dependencyScope.GetService(typeof(object));

            Assert.Null(service);
        }

        [Fact]
        public void GetServiceReturnsRegisteredService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).InstancePerRequest();
            var lifetimeScope = builder.Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);

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
            var resolver = new AutofacWebApiDependencyScope(lifetimeScope);

            var services = resolver.GetServices(typeof(object));

            Assert.Equal(2, services.Count());
        }

        [Fact]
        public void GetServicesReturnsEmptyEnumerableForUnregisteredService()
        {
            var lifetimeScope = new ContainerBuilder().Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);

            var services = dependencyScope.GetServices(typeof(object));

            Assert.Empty(services);
        }

        [Fact]
        public void GetServicesReturnsRegisteredService()
        {
            var builder = new ContainerBuilder();
            builder.Register(c => new object()).InstancePerRequest();
            var lifetimeScope = builder.Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            var dependencyScope = new AutofacWebApiDependencyScope(lifetimeScope);

            var services = dependencyScope.GetServices(typeof(object));

            Assert.Single(services);
        }
    }
}