using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi.Test.TestTypes;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public abstract class AutofacFilterBaseFixture<TFilter1, TFilter2, TFilterType>
    {
        [Fact]
        public void ResolvesControllerScopedFilter()
        {
            AssertSingleFilter<TestController>(
                GetFirstRegistration(),
                ConfigureFirstControllerRegistration());
        }

        [Fact]
        public void ResolvesActionScopedFilter()
        {
            AssertSingleFilter<TestController>(
                GetFirstRegistration(),
                ConfigureFirstActionRegistration());
        }

        [Fact]
        public void ResolvesActionScopedFilterForImmediateBaseController()
        {
            AssertSingleFilter<TestControllerA>(
                GetFirstRegistration(),
                ConfigureFirstActionRegistration());
        }

        [Fact]
        public void ResolvesActionScopedFilterForMostBaseController()
        {
            AssertSingleFilter<TestControllerB>(
                GetFirstRegistration(),
                ConfigureFirstActionRegistration());
        }

        [Fact]
        public void ResolvesMultipleControllerScopedFilters()
        {
            AssertMultipleFilters(
                GetFirstRegistration(),
                GetSecondRegistration(),
                ConfigureFirstControllerRegistration(),
                ConfigureSecondControllerRegistration());
        }

        [Fact]
        public void ResolvesMultipleActionScopedFilters()
        {
            AssertMultipleFilters(
                GetFirstRegistration(),
                GetSecondRegistration(),
                ConfigureFirstActionRegistration(),
                ConfigureSecondActionRegistration());
        }

        [Fact]
        public void ResolvesControllerScopedOverrideFilter()
        {
            AssertOverrideFilter<TestController>(
                ConfigureControllerFilterOverride());
        }

        [Fact]
        public void ResolvesActionScopedOverrideFilter()
        {
            AssertOverrideFilter<TestController>(
                ConfigureActionFilterOverride());
        }

        [Fact]
        public void ResolvesActionScopedOverrideFilterForImmediateBaseController()
        {
            AssertOverrideFilter<TestControllerA>(
                ConfigureActionFilterOverride());
        }

        [Fact]
        public void ResolvesActionScopedOverrideFilterForMostBaseController()
        {
            AssertOverrideFilter<TestControllerB>(
                ConfigureActionFilterOverride());
        }

        [Fact]
        public void ResolvesControllerScopedOverrideFilterForImmediateBaseController()
        {
            AssertOverrideFilter<TestControllerA>(
                ConfigureControllerFilterOverride());
        }

        [Fact]
        public void ResolvesControllerScopedOverrideFilterForMostBaseController()
        {
            AssertOverrideFilter<TestControllerB>(
                ConfigureControllerFilterOverride());
        }

        [Fact]
        public void ResolvesRegisteredActionFilterOverrideForAction()
        {
            AssertOverrideFilter<TestController>(
                GetFirstRegistration(),
                ConfigureActionOverrideRegistration());
        }

        [Fact]
        public void ResolvesRegisteredActionFilterOverrideForController()
        {
            AssertOverrideFilter<TestController>(
                GetFirstRegistration(),
                ConfigureControllerOverrideRegistration());
        }

        [Fact]
        public void ResolvesRegisteredActionFilterForAllControllers()
        {
            AssertMultiControllerRegistration<TestControllerA, TestControllerB>(
                GetFirstRegistration(),
                ConfigureFirstAllControllersRegistration());
        }

        [Fact]
        public void ResolvesRegisteredActionFilterForMultipleAllControllersRegistration()
        {
            AssertMultiControllerRegistration<TestControllerA, TestControllerB>(
                GetFirstRegistration(),
                GetSecondRegistration(),
                ConfigureFirstAllControllersRegistration(),
                ConfigureSecondAllControllersRegistration());
        }

        [Fact]
        public void ResolvesRegisteredActionFilterForChainedControllers()
        {
            AssertMultiControllerRegistration<TestControllerA, TestControllerB>(
                GetFirstRegistration(),
                ConfigureFirstChainedControllersRegistration());
        }

        [Fact]
        public void ResolvesRegisteredActionFilterForMultipleChainedControllersRegistration()
        {
            AssertMultiControllerRegistration<TestControllerA, TestControllerB>(
                GetFirstRegistration(),
                GetSecondRegistration(),
                ConfigureFirstChainedControllersRegistration(),
                ConfigureSecondChainedControllersRegistration());
        }

        [Fact]
        public void ResolvesRegisteredActionFilterForPredicateRegistration()
        {
            AssertSingleFilter<TestControllerA>(
                GetFirstRegistration(),
                ConfigureFirstPredicateRegistration((scope, descriptor) => descriptor.ControllerDescriptor.ControllerType == typeof(TestControllerA)));
        }

        [Fact]
        public void ResolvesRegisteredActionFilterForMultiplePredicateRegistration()
        {
            AssertMultipleFilters(
                GetFirstRegistration(),
                GetSecondRegistration(),
                ConfigureFirstPredicateRegistration((scope, descriptor) => descriptor.ControllerDescriptor.ControllerType == typeof(TestController)),
                ConfigureSecondPredicateRegistration(descriptor => descriptor.ControllerDescriptor.ControllerType == typeof(TestController)));
        }

        [Fact]
        public void DoesNotResolveRegisteredActionFilterForNonMatchingPredicateRegistration()
        {
            AssertNoFilter<TestControllerB>(
                GetFirstRegistration(),
                ConfigureFirstPredicateRegistration((scope, descriptor) => descriptor.ControllerDescriptor.ControllerType == typeof(TestControllerA)));
        }

        [Fact]
        public void CanUseLifetimeScopeInPredicate()
        {
            AssertNoFilter<TestControllerB>(
                GetFirstRegistration(),
                ConfigureFirstPredicateRegistration((scope, descriptor) =>
                {
                    Assert.NotNull(scope.Resolve<ILogger>());

                    return descriptor.ControllerDescriptor.ControllerType == typeof(TestControllerA);
                }));
        }

        protected abstract Func<IComponentContext, TFilter1> GetFirstRegistration();

        protected abstract Func<IComponentContext, TFilter2> GetSecondRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstAllControllersRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstChainedControllersRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstPredicateRegistration(Func<ILifetimeScope, HttpActionDescriptor, bool> predicate);

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondAllControllersRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondChainedControllersRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondPredicateRegistration(Func<HttpActionDescriptor, bool> predicate);

        protected abstract Type GetWrapperType();

        protected abstract Type GetOverrideWrapperType();

        protected abstract Action<ContainerBuilder> ConfigureControllerFilterOverride();

        protected abstract Action<ContainerBuilder> ConfigureActionFilterOverride();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureActionOverrideRegistration();

        protected abstract Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> ConfigureControllerOverrideRegistration();

        private static ReflectedHttpActionDescriptor BuildActionDescriptorForGetMethod()
        {
            return BuildActionDescriptorForGetMethod(typeof(TestController));
        }

        private static ReflectedHttpActionDescriptor BuildActionDescriptorForGetMethod(Type controllerType)
        {
            var controllerDescriptor = new HttpControllerDescriptor { ControllerType = controllerType };
            var methodInfo = controllerType.GetMethod("Get");
            return new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
        }

        private void AssertSingleFilter<TController>(
            Func<IComponentContext, TFilter1> registration,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure(builder.Register(registration).InstancePerRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController));

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var wrapperType = GetWrapperType();
            var filter = filterInfos.Select(info => info.Instance).Single(i => i.GetType() == wrapperType);
            Assert.IsType(wrapperType, filter);
        }

        private void AssertNoFilter<TController>(
            Func<IComponentContext, TFilter1> registration,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure(builder.Register(registration).InstancePerRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController));

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var wrapperType = GetWrapperType();
            var filterApplied = filterInfos.Select(info => info.Instance).Any(i => i.GetType() == wrapperType);
            Assert.False(filterApplied);
        }

        private void AssertMultipleFilters(
            Func<IComponentContext, TFilter1> registration1,
            Func<IComponentContext, TFilter2> registration2,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure1,
            Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> configure2)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure1(builder.Register(registration1).InstancePerRequest());
            configure2(builder.Register(registration2).InstancePerRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod();

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var wrapperType = GetWrapperType();
            var filters = filterInfos.Select(info => info.Instance).Where(i => i.GetType() == wrapperType).ToArray();
            Assert.Single(filters);
            Assert.IsType(wrapperType, filters[0]);
        }

        private static void AssertOverrideFilter<TController>(Action<ContainerBuilder> registration)
        {
            var builder = new ContainerBuilder();
            registration(builder);
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController));

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var filter = filterInfos.Select(info => info.Instance).OfType<AutofacOverrideFilter>().Single();
            Assert.IsType<AutofacOverrideFilter>(filter);
            Assert.False(filter.AllowMultiple);
            Assert.Equal(typeof(TFilterType), filter.FiltersToOverride);
        }

        private void AssertOverrideFilter<TController>(
            Func<IComponentContext, TFilter1> registration,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure(builder.Register(registration).InstancePerRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var actionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController));

            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            var wrapperType = GetOverrideWrapperType();
            var filters = filterInfos.Select(info => info.Instance).Where(i => i.GetType() == wrapperType).ToArray();
            Assert.Single(filters);
            Assert.IsType(wrapperType, filters[0]);
        }

        private void AssertMultiControllerRegistration<TController1, TController2>(
            Func<IComponentContext, TFilter1> registration,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure(builder.Register(registration).InstancePerRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };

            var wrapperType = GetWrapperType();

            var firstActionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController1));

            var firstControllerFilters = provider.GetFilters(configuration, firstActionDescriptor).ToArray();

            var firstFilter = firstControllerFilters.Select(info => info.Instance).Single(i => i.GetType() == wrapperType);

            Assert.IsType(wrapperType, firstFilter);

            var secondActionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController2));

            var secondControllerFilters = provider.GetFilters(configuration, secondActionDescriptor).ToArray();

            var secondFilter = secondControllerFilters.Select(info => info.Instance).Single(i => i.GetType() == wrapperType);

            Assert.IsType(wrapperType, secondFilter);
        }

        private void AssertMultiControllerRegistration<TController1, TController2>(
            Func<IComponentContext, TFilter1> registration1,
            Func<IComponentContext, TFilter2> registration2,
            Action<IRegistrationBuilder<TFilter1, SimpleActivatorData, SingleRegistrationStyle>> configure1,
            Action<IRegistrationBuilder<TFilter2, SimpleActivatorData, SingleRegistrationStyle>> configure2)
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            configure1(builder.Register(registration1).InstancePerRequest());
            configure2(builder.Register(registration2).InstancePerRequest());
            var container = builder.Build();
            var provider = new AutofacWebApiFilterProvider(container);
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };

            var wrapperType = GetWrapperType();

            var firstActionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController1));

            var firstControllerFilters = provider.GetFilters(configuration, firstActionDescriptor).ToArray();

            var firstFilter = firstControllerFilters.Select(info => info.Instance).Single(i => i.GetType() == wrapperType);

            Assert.IsType(wrapperType, firstFilter);

            var secondActionDescriptor = BuildActionDescriptorForGetMethod(typeof(TController2));

            var secondControllerFilters = provider.GetFilters(configuration, secondActionDescriptor).ToArray();

            var secondFilter = secondControllerFilters.Select(info => info.Instance).Single(i => i.GetType() == wrapperType);

            Assert.IsType(wrapperType, secondFilter);
        }
    }
}
