using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Moq;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class AutofacControllerConfigurationAttributeFixture
    {
        [Fact]
        public void ExceptionThrownWhenAutofacDependencyResolverMissing()
        {
            var attribute = new AutofacControllerConfigurationAttribute();
            var configuration = new HttpConfiguration();
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));

            Assert.Throws<InvalidOperationException>(() => attribute.Initialize(settings, descriptor));
        }

        [Fact]
        public void HandlesMissingHttpConfiguration()
        {
            var attribute = new AutofacControllerConfigurationAttribute();
            var settings = new HttpControllerSettings(new HttpConfiguration());
            var descriptor = new HttpControllerDescriptor();

            // XUnit doesn't have Assert.DoesNotThrow
            attribute.Initialize(settings, descriptor);
        }

        [Fact]
        public void IntializationRunOncePerControllerType()
        {
            var builder = new ContainerBuilder();
            var service = new Mock<IHttpActionSelector>().Object;
            int callCount = 0;
            builder.Register(c => service)
                .As<IHttpActionSelector>()
                .InstancePerApiControllerType(typeof(TestController))
                .OnActivated(e => callCount++);
            var container = builder.Build();
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);
            attribute.Initialize(settings, descriptor);

            Assert.Equal(1, callCount);
        }


        [Fact]
        public void PerControllerServiceDoesNotOverrideDefault()
        {
            var builder = new ContainerBuilder();
            var service = new Mock<IHttpActionSelector>().Object;
            builder.Register(c => service)
                .As<IHttpActionSelector>()
                .InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            var configuration = new HttpConfiguration { DependencyResolver = resolver };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.NotSame(service, configuration.Services.GetActionSelector());
        }

        [Fact]
        public void UsesDefaultServiceWhenNoKeyedServiceRegistered()
        {
            var builder = new ContainerBuilder();
            var container = builder.Build();
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.IsType<ApiControllerActionSelector>(settings.Services.GetActionSelector());
        }

        [Fact]
        public void UsesRootServiceWhenNoKeyedServiceRegistered()
        {
            var builder = new ContainerBuilder();
            var service = new Mock<IHttpActionSelector>().Object;
            builder.RegisterInstance(service);
            var container = builder.Build();
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.Equal(service, settings.Services.GetActionSelector());
        }

        [Fact]
        public void RegistrationForBaseControllerAppliesForDerived()
        {
            var builder = new ContainerBuilder();
            var service = new Mock<IHttpActionSelector>().Object;
            builder.Register(c => service)
                .As<IHttpActionSelector>()
                .InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var resolver = new AutofacWebApiDependencyResolver(container);
            var configuration = new HttpConfiguration { DependencyResolver = resolver };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestControllerA", typeof(TestControllerA));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.Equal(service, settings.Services.GetActionSelector());
        }

        [Fact]
        public void FormattersCanBeResolvedPerControllerType()
        {
            var builder = new ContainerBuilder();
            var formatter1 = new Mock<MediaTypeFormatter>().Object;
            var formatter2 = new Mock<MediaTypeFormatter>().Object;
            builder.RegisterInstance(formatter1).InstancePerApiControllerType(typeof(TestController));
            builder.RegisterInstance(formatter2).InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.Equal(6, settings.Formatters.Count);
            Assert.Contains(formatter1, settings.Formatters);
            Assert.Contains(formatter2, settings.Formatters);
        }

        [Fact]
        public void ExistingFormattersCanBeCleared()
        {
            var builder = new ContainerBuilder();
            var formatter1 = new Mock<MediaTypeFormatter>().Object;
            var formatter2 = new Mock<MediaTypeFormatter>().Object;
            builder.RegisterInstance(formatter1).InstancePerApiControllerType(typeof(TestController), true);
            builder.RegisterInstance(formatter2).InstancePerApiControllerType(typeof(TestController), true);
            var container = builder.Build();
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.Equal(2, settings.Formatters.Count);
            Assert.Contains(formatter1, settings.Formatters);
            Assert.Contains(formatter2, settings.Formatters);
        }

        [Fact]
        public void ExistingListServicesCanBeCleared()
        {
            var builder = new ContainerBuilder();
            var provider1 = new Mock<ModelBinderProvider>().Object;
            var provider2 = new Mock<ModelBinderProvider>().Object;
            builder.RegisterInstance(provider1).InstancePerApiControllerType(typeof(TestController), true);
            builder.RegisterInstance(provider2).InstancePerApiControllerType(typeof(TestController), true);
            var container = builder.Build();
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);

            var services = settings.Services.GetServices(typeof(ModelBinderProvider)).ToArray();

            Assert.Equal(2, services.Count());
            Assert.Contains(provider1, services);
            Assert.Contains(provider2, services);
        }

        [Fact]
        public void SupportsActionInvoker()
        {
            AssertControllerServiceReplaced(services => services.GetActionInvoker());
        }

        [Fact]
        public void SupportsActionSelector()
        {
            AssertControllerServiceReplaced(services => services.GetActionSelector());
        }

        [Fact]
        public void SupportsActionValueBinder()
        {
            AssertControllerServiceReplaced(services => services.GetActionValueBinder());
        }

        [Fact]
        public void SupportsBodyModelValidator()
        {
            AssertControllerServiceReplaced(services => services.GetBodyModelValidator());
        }

        [Fact]
        public void SupportsContentNegotiator()
        {
            AssertControllerServiceReplaced(services => services.GetContentNegotiator());
        }

        [Fact]
        public void SupportsHttpControllerActivator()
        {
            AssertControllerServiceReplaced(services => services.GetHttpControllerActivator());
        }

        [Fact]
        public void SupportsModelMetadataProvider()
        {
            AssertControllerServiceReplaced(services => services.GetHttpControllerActivator());
        }

        [Fact]
        public void SupportsModelBinderProviders()
        {
            AssertControllerServicesReplaced(services => services.GetModelBinderProviders());
        }

        [Fact]
        public void SupportsModelValidatorProviders()
        {
            AssertControllerServicesReplaced(services => services.GetModelValidatorProviders());
        }

        [Fact]
        public void SupportsValueProviderFactories()
        {
            AssertControllerServicesReplaced(services => services.GetValueProviderFactories());
        }

        static void AssertControllerServiceReplaced<TLimit>(Func<ServicesContainer, TLimit> serviceLocator)
            where TLimit : class
        {
            var builder = new ContainerBuilder();
            var service = new Mock<TLimit>().Object;
            builder.RegisterInstance(service).As<TLimit>().InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);
            Assert.Same(service, serviceLocator(settings.Services));
            Assert.NotSame(service, serviceLocator(configuration.Services));
        }

        static void AssertControllerServicesReplaced<TLimit>(Func<ServicesContainer, IEnumerable<TLimit>> serviceLocator)
            where TLimit : class
        {
            var builder = new ContainerBuilder();
            var service = new Mock<TLimit>().Object;
            builder.RegisterInstance(service).As<TLimit>().InstancePerApiControllerType(typeof(TestController));
            var container = builder.Build();
            var configuration = new HttpConfiguration { DependencyResolver = new AutofacWebApiDependencyResolver(container) };
            var settings = new HttpControllerSettings(configuration);
            var descriptor = new HttpControllerDescriptor(configuration, "TestController", typeof(TestController));
            var attribute = new AutofacControllerConfigurationAttribute();

            attribute.Initialize(settings, descriptor);

            Assert.Contains(service, serviceLocator(settings.Services));
            Assert.DoesNotContain(service, serviceLocator(configuration.Services));
        }
    }
}