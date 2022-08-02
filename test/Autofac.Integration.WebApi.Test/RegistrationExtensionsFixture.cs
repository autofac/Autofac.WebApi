// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Autofac.Builder;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test
{
    public class RegistrationExtensionsFixture
    {
        [Fact]
        public void RegisterApiControllersRegistersTypesWithControllerSuffix()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            Assert.True(container.IsRegistered<TestController>());
        }

        [Fact]
        public void RegisterApiControllersIgnoresTypesWithoutControllerSuffixByDefault()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            Assert.False(container.IsRegistered<IsAControllerNot>());
        }

        [Fact]
        public void RegisterApiControllersAllowsCustomSuffixToSpecified()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers("WithSuffix", Assembly.GetExecutingAssembly());

            var container = builder.Build();
            Assert.True(container.IsRegistered<TestControllerWithSuffix>());
        }

        [Fact]
        public void RegisterApiControllersFindsTypesImplemtingInterfaceOnly()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            Assert.True(container.IsRegistered<InterfaceController>());
        }

        [Fact]
        public void InstancePerApiControllerTypeRequiresControllerTypeParameter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterType<object>().InstancePerApiControllerType(null));

            Assert.Equal("controllerType", exception.ParamName);
        }

        [Fact]
        public void RegisterWebApiModelBinderProviderThrowsExceptionForNullBuilder()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.WebApi.RegistrationExtensions.RegisterWebApiModelBinderProvider(null));
            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void RegisterWebApiModelBinderProviderRegistersSingleInstanceProvider()
        {
            var builder = new ContainerBuilder();
            builder.RegisterWebApiModelBinderProvider();
            using var config = new HttpConfiguration();
            builder.RegisterInstance(config);
            var container = builder.Build();

            var resolvedProvider1 = container.Resolve<ModelBinderProvider>();
            var resolvedProvider2 = container.Resolve<ModelBinderProvider>();

            Assert.Same(resolvedProvider2, resolvedProvider1);
        }

        [Fact]
        public void AsModelBinderForTypesThrowsExceptionWhenAllTypesNullInList()
        {
            var builder = new ContainerBuilder();
            var registration = builder.RegisterType<TestModelBinder>();
            Assert.Throws<ArgumentException>(() => registration.AsModelBinderForTypes(null, null, null));
        }

        [Fact]
        public void AsModelBinderForTypesThrowsExceptionForEmptyTypeList()
        {
            var types = Array.Empty<Type>();
            var builder = new ContainerBuilder();
            var registration = builder.RegisterType<TestModelBinder>();
            Assert.Throws<ArgumentException>(() => registration.AsModelBinderForTypes(types));
        }

        [Fact]
        public void AsModelBinderForTypesRegistersInstanceModelBinder()
        {
            var builder = new ContainerBuilder();
            var binder = new TestModelBinder(new Dependency());
            builder.RegisterInstance(binder).AsModelBinderForTypes(typeof(TestModel1));
            var container = builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            using var configuration = new HttpConfiguration { DependencyResolver = resolver };
            var provider = new AutofacWebApiModelBinderProvider();
            Assert.Same(binder, provider.GetBinder(configuration, typeof(TestModel1)));
        }

        [Fact]
        public void AsModelBinderForTypesRegistersTypeModelBinder()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Dependency>();
            builder.RegisterType<TestModelBinder>().AsModelBinderForTypes(typeof(TestModel1), typeof(TestModel2));
            var container = builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            using var configuration = new HttpConfiguration { DependencyResolver = resolver };
            var provider = new AutofacWebApiModelBinderProvider();

            Assert.IsType<TestModelBinder>(provider.GetBinder(configuration, typeof(TestModel1)));
            Assert.IsType<TestModelBinder>(provider.GetBinder(configuration, typeof(TestModel2)));
        }

        [Fact]
        public void AsModelBinderForTypesThrowsExceptionForNullRegistration()
        {
            IRegistrationBuilder<RegistrationExtensionsFixture, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration = null;
            Assert.Throws<ArgumentNullException>(() => registration.AsModelBinderForTypes(typeof(TestModel1)));
        }

        [Fact]
        public void AsModelBinderForTypesThrowsExceptionForNullTypeList()
        {
            Type[] types = null;
            var builder = new ContainerBuilder();
            var registration = builder.RegisterType<TestModelBinder>();
            Assert.Throws<ArgumentNullException>(() => registration.AsModelBinderForTypes(types));
        }

        [Fact]
        public void InstancePerApiControllerTypeRequiresTypeParameter()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.RegisterType<object>().InstancePerApiControllerType(null));

            Assert.Equal("controllerType", exception.ParamName);
        }

        [Fact]
        public void InstancePerApiControllerTypeAddsKeyedRegistration()
        {
            var controllerType = typeof(TestController);
            var serviceKey = new ControllerTypeKey(controllerType);

            var builder = new ContainerBuilder();
            builder.RegisterType<object>().InstancePerApiControllerType(controllerType);
            var container = builder.Build();

            Assert.True(container.IsRegisteredWithKey<object>(serviceKey));
        }

        [Fact]
        public void RegisterHttpRequestMessageAddsHandler()
        {
            using var config = new HttpConfiguration();
            var builder = new ContainerBuilder();
            builder.RegisterHttpRequestMessage(config);

            Assert.Single(config.MessageHandlers.OfType<CurrentRequestHandler>());
        }

        [Fact]
        public void RegisterHttpRequestMessageThrowsGivenNullConfig()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentNullException>(() => builder.RegisterHttpRequestMessage(null));

            Assert.Equal("config", exception.ParamName);
        }

        [Fact]
        public void RegisterHttpRequestMessageEnsuresHandlerAddedOnlyOnce()
        {
            using var config = new HttpConfiguration();
            var builder = new ContainerBuilder();

            builder.RegisterHttpRequestMessage(config);
            builder.RegisterHttpRequestMessage(config);

            Assert.Single(config.MessageHandlers.OfType<CurrentRequestHandler>());
        }

        [Fact]
        public void AsActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>())).AsWebApiActionFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void AsActionFilterForServiceTypeMustBeActionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiActionFilterFor<TestController>());

            Assert.Equal("registration", exception.ParamName);
        }

        [Fact]
        public void AsActionFilterForPredicateMustBeActionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiActionFilterWhere(descriptor => true));

            Assert.Equal("registration", exception.ParamName);
        }

        [Fact]
        public void AsActionFilterForPredicateNoGlobalScope()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<InvalidEnumArgumentException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>())).AsWebApiActionFilterWhere(descriptor => true, FilterScope.Global));

            Assert.Equal("filterScope", exception.ParamName);
        }

        [Fact]
        public void AsActionFilterForAllControllersMustBeActionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiActionFilterForAllControllers());

            Assert.Equal("registration", exception.ParamName);
            Assert.StartsWith(
                "The type 'System.Object' must be assignable to 'Autofac.Integration.WebApi.IAutofacActionFilter'" +
                " or 'Autofac.Integration.WebApi.IAutofacContinuationActionFilter'.",
                exception.Message,
                StringComparison.CurrentCulture);
        }

        [Fact]
        public void AsActionFilterWhereRequiresPredicate()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>())).AsWebApiActionFilterWhere((Func<ILifetimeScope, HttpActionDescriptor, bool>)null));
            Assert.Equal("predicate", exception.ParamName);
        }

        [Fact]
        public void CanRegisterMultipleFilterTypesAgainstSingleService()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new TestCombinationFilter())
                .AsWebApiActionFilterFor<TestController>()
                .AsWebApiAuthenticationFilterFor<TestController>()
                .AsWebApiAuthorizationFilterFor<TestController>()
                .AsWebApiExceptionFilterFor<TestController>();

            using var configuration = new HttpConfiguration();
            builder.RegisterWebApiFilterProvider(configuration);
            var container = builder.Build();

            Assert.NotNull(container.Resolve<IAutofacContinuationActionFilter>());
            Assert.NotNull(container.Resolve<IAutofacAuthenticationFilter>());
            Assert.NotNull(container.Resolve<IAutofacAuthorizationFilter>());
            Assert.NotNull(container.Resolve<IAutofacExceptionFilter>());
        }

        [Fact]
        public void AutofacActionFilterIsAdapted()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(new TestCombinationFilter())
                .AsWebApiActionFilterFor<TestController>();

            using var configuration = new HttpConfiguration();

            builder.RegisterWebApiFilterProvider(configuration);

            var container = builder.Build();

            Assert.NotNull(container.Resolve<IAutofacContinuationActionFilter>());
        }

        [Fact]
        public void AsAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthorizationFilter(c.Resolve<ILogger>())).AsWebApiAuthorizationFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void AsWebApiAuthorizationFilterForServiceTypeMustBeAuthorizationFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthorizationFilterFor<TestController>());

            Assert.Equal("registration", exception.ParamName);
        }

        [Fact]
        public void AsAuthorizationFilterWhereRequiresPredicate()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>()))
                             .AsWebApiAuthorizationFilterWhere((Func<ILifetimeScope, HttpActionDescriptor, bool>)null));
            Assert.Equal("predicate", exception.ParamName);
        }

        [Fact]
        public void AsExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestExceptionFilter(c.Resolve<ILogger>())).AsWebApiExceptionFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void AsExceptionFilterWhereRequiresPredicate()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>()))
                             .AsWebApiExceptionFilterWhere((Func<ILifetimeScope, HttpActionDescriptor, bool>)null));
            Assert.Equal("predicate", exception.ParamName);
        }

        [Fact]
        public void AsWebApiAuthorizationFilterForServiceTypeMustBeExceptionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthorizationFilterFor<TestController>());

            Assert.Equal("registration", exception.ParamName);
        }

        [Fact]
        public void AsAuthenticationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthenticationFilter(c.Resolve<ILogger>())).AsWebApiAuthenticationFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void AsAuthenticationFilterWhereRequiresPredicate()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestActionFilter(c.Resolve<ILogger>()))
                             .AsWebApiAuthorizationFilterWhere((Func<ILifetimeScope, HttpActionDescriptor, bool>)null));
            Assert.Equal("predicate", exception.ParamName);
        }

        [Fact]
        public void AsWebApiAuthenticationFilterForServiceTypeMustBeAuthenticationFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthenticationFilterFor<TestController>());

            Assert.Equal("registration", exception.ParamName);
        }

        [Fact]
        public void OverrideActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiActionFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void OverrideAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiAuthorizationFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void OverrideExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiExceptionFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void OverrideAuthenticationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiAuthenticationFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }
    }
}
