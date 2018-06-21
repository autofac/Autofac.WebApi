using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class AutofacWebApiModelBinderProviderFixture
    {
        [Fact]
        public void ProviderIsRegisteredAsSingleInstance()
        {
            var container = BuildContainer();
            var provider1 = container.Resolve<ModelBinderProvider>();
            var provider2 = container.Resolve<ModelBinderProvider>();
            Assert.Same(provider2, provider1);
        }

        [Fact]
        public void ModelBindersAreRegistered()
        {
            var container = BuildContainer();
            var modelBinders = container.Resolve<IEnumerable<IModelBinder>>();
            Assert.Single(modelBinders);
        }

        [Fact]
        public void ModelBinderHasDependenciesInjected()
        {
            var container = BuildContainer();
            var modelBinder = container.Resolve<IEnumerable<IModelBinder>>()
                .OfType<TestModelBinder>()
                .FirstOrDefault();
            Assert.NotNull(modelBinder);
            Assert.NotNull(modelBinder.Dependency);
        }

        [Fact]
        public void ReturnsNullWhenModelBinderRegisteredWithoutMetadata()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Dependency>().AsSelf();
            builder.RegisterWebApiModelBinderProvider();
            builder.RegisterType<TestModelBinder>().As<IModelBinder>();
            var container = builder.Build();

            var modelBinders = container.Resolve<IEnumerable<IModelBinder>>().ToList();
            Assert.Single(modelBinders);
            Assert.IsType<TestModelBinder>(modelBinders.First());

            var provider = container.Resolve<ModelBinderProvider>();

            Assert.Null(provider.GetBinder(new HttpConfiguration(), typeof(TestModel1)));
        }

        static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<Dependency>().AsSelf();
            builder.RegisterWebApiModelBinderProvider();
            builder.RegisterType<TestModelBinder>().AsModelBinderForTypes(typeof(TestModel1));

            return builder.Build();
        }
    }
}