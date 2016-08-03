using System;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class AuthorizationFilterWrapperFixture
    {
        [Fact]
        public void RequiresFilterMetadata()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new AuthorizationFilterWrapper(null));
            Assert.Equal("filterMetadata", exception.ParamName);
        }

        [Fact]
        public async void WrapperResolvesAuthorizationFilterFromDependencyScope()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            var activationCount = 0;
            builder.Register<IAutofacAuthorizationFilter>(c => new TestAuthorizationFilter(c.Resolve<ILogger>()))
                .AsWebApiAuthorizationFilterFor<TestController>(c => c.Get())
                .InstancePerRequest()
                .OnActivated(e => activationCount++);
            var container = builder.Build();

            var resolver = new AutofacWebApiDependencyResolver(container);
            var configuration = new HttpConfiguration { DependencyResolver = resolver };
            var requestMessage = new HttpRequestMessage();
            requestMessage.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, configuration);
            var contollerContext = new HttpControllerContext { Request = requestMessage };
            var controllerDescriptor = new HttpControllerDescriptor { ControllerType = typeof(TestController) };
            var methodInfo = typeof(TestController).GetMethod("Get");
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            var actionContext = new HttpActionContext(contollerContext, actionDescriptor);
            var metadata = new FilterMetadata
            {
                ControllerType = typeof(TestController),
                FilterScope = FilterScope.Action,
                MethodInfo = methodInfo
            };
            var wrapper = new AuthorizationFilterWrapper(metadata);

            await wrapper.OnAuthorizationAsync(actionContext, new CancellationToken());
            Assert.Equal(1, activationCount);
        }
    }
}