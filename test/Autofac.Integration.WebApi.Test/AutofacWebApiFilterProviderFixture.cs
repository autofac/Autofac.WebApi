// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http;
using System.Web.Http.Controllers;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test
{
    public class AutofacWebApiFilterProviderFixture
    {
        [Fact]
        public void FilterRegistrationsWithoutMetadataIgnored()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<AuthorizeAttribute>().AsImplementedInterfaces();
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            using var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor);

            Assert.False(filterInfos.Select(f => f.Instance).OfType<AuthorizeAttribute>().Any());
        }

        [Fact]
        public void InjectsFilterPropertiesForRegisteredDependencies()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            using var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filter = filterInfos.Select(info => info.Instance).OfType<CustomActionFilterAttribute>().Single();
            Assert.IsAssignableFrom<ILogger>(filter.Logger);
        }

        [Fact]
        public void ReturnsFiltersWithoutPropertyInjectionForUnregisteredDependencies()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            using var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filter = filterInfos.Select(info => info.Instance).OfType<CustomActionFilterAttribute>().Single();
            Assert.Null(filter.Logger);
        }

        [Fact]
        public void ResolvesMultipleFiltersOfDifferentTypes()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();

            builder.Register(c => new TestAuthenticationFilter(c.Resolve<ILogger>()))
                .AsWebApiAuthenticationFilterFor<TestController>()
                .InstancePerRequest();

            builder.Register(c => new TestAuthorizationFilter(c.Resolve<ILogger>()))
                .AsWebApiAuthorizationFilterFor<TestController>()
                .InstancePerRequest();

            builder.Register(c => new TestExceptionFilter(c.Resolve<ILogger>()))
                .AsWebApiExceptionFilterFor<TestController>()
                .InstancePerRequest();

            builder.Register(c => new TestActionFilter(c.Resolve<ILogger>()))
                .AsWebApiActionFilterFor<TestController>()
                .InstancePerRequest();

            using var configuration = new HttpConfiguration();
            builder.RegisterWebApiFilterProvider(configuration);
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();
            var filters = filterInfos.Select(info => info.Instance).ToArray();

            Assert.Single(filters.OfType<AuthenticationFilterWrapper>());
            Assert.Single(filters.OfType<AuthorizationFilterWrapper>());
            Assert.Single(filters.OfType<ExceptionFilterWrapper>());
            Assert.Single(filters.OfType<ContinuationActionFilterWrapper>());
        }

        private static ReflectedHttpActionDescriptor BuildActionDescriptorForGetMethod()
        {
            return BuildActionDescriptorForGetMethod(typeof(TestController));
        }

        private static ReflectedHttpActionDescriptor BuildActionDescriptorForGetMethod(Type controllerType)
        {
            var controllerDescriptor = new HttpControllerDescriptor { ControllerType = controllerType };
            var methodInfo = typeof(TestController).GetMethod("Get");
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            return actionDescriptor;
        }
    }
}
