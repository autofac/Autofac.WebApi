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
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Autofac.Builder;
using Xunit;

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
        public void RegisterApiControllersIgnoresTypesWithoutControllerSuffix()
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            var container = builder.Build();
            Assert.False(container.IsRegistered<IsAControllerNot>());
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
            builder.RegisterInstance(new HttpConfiguration());
            var container = builder.Build();

            var resolvedProvider1 = container.Resolve<ModelBinderProvider>();
            var resolvedProvider2 = container.Resolve<ModelBinderProvider>();

            Assert.Same(resolvedProvider2, resolvedProvider1);
        }

        [Fact]
        public void RegisterWebApiModelBindersThrowsExceptionForNullBuilder()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => Autofac.Integration.WebApi.RegistrationExtensions.RegisterWebApiModelBinders(null, Assembly.GetExecutingAssembly()));
            Assert.Equal("builder", exception.ParamName);
        }

        [Fact]
        public void RegisterWebApiModelBindersThrowsExceptionForNullAssemblies()
        {
            var exception = Assert.Throws<ArgumentNullException>(
                () => new ContainerBuilder().RegisterWebApiModelBinders(null));
            Assert.Equal("modelBinderAssemblies", exception.ParamName);
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
            var types = new Type[0];
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
            var configuration = new HttpConfiguration { DependencyResolver = resolver };
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
            var configuration = new HttpConfiguration { DependencyResolver = resolver };
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
            var config = new HttpConfiguration();
            var builder = new ContainerBuilder();
            builder.RegisterHttpRequestMessage(config);

            Assert.Equal(1, config.MessageHandlers.OfType<CurrentRequestHandler>().Count());
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
            var config = new HttpConfiguration();
            var builder = new ContainerBuilder();

            builder.RegisterHttpRequestMessage(config);
            builder.RegisterHttpRequestMessage(config);

            Assert.Equal(1, config.MessageHandlers.OfType<CurrentRequestHandler>().Count());
        }


        // Action filters

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

        // Authorization filter

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

        // Exception filters

        [Fact]
        public void AsExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestExceptionFilter(c.Resolve<ILogger>())).AsWebApiExceptionFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void AsWebApiAuthorizationFilterForServiceTypeMustBeExceptionFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthorizationFilterFor<TestController>());

            Assert.Equal("registration", exception.ParamName);
        }

        // Authentication filters

        [Fact]
        public void AsAuthenticationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.Register(c => new TestAuthenticationFilter(c.Resolve<ILogger>())).AsWebApiAuthenticationFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        [Fact]
        public void AsWebApiAuthenticationFilterForServiceTypeMustBeAuthenticationFilter()
        {
            var builder = new ContainerBuilder();

            var exception = Assert.Throws<ArgumentException>(
                () => builder.RegisterInstance(new object()).AsWebApiAuthenticationFilterFor<TestController>());

            Assert.Equal("registration", exception.ParamName);
        }

        // Action filter override

        [Fact]
        public void OverrideActionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiActionFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        // Authorization filter override

        [Fact]
        public void OverrideAuthorizationFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiAuthorizationFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        // Exception filter override

        [Fact]
        public void OverrideExceptionFilterForRequiresActionSelector()
        {
            var builder = new ContainerBuilder();
            var exception = Assert.Throws<ArgumentNullException>(
                () => builder.OverrideWebApiExceptionFilterFor<TestController>(null));
            Assert.Equal("actionSelector", exception.ParamName);
        }

        // Authentication filter override

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