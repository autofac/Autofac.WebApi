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
    public class AuthenticationFilterWrapperFixture
    {
        [Fact]
        public void RequiresFilterMetadata()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new AuthenticationFilterWrapper(null));
            Assert.Equal("filterMetadata", exception.ParamName);
        }

        [Fact]
        public void WrapperResolvesAuthenticationFilterFromDependencyScope()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            var activationCount = 0;
            builder.Register<IAutofacAuthenticationFilter>(c => new TestAuthenticationFilter(c.Resolve<ILogger>()))
                .AsWebApiAuthenticationFilterFor<TestController>(c => c.Get())
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
            var context = new HttpAuthenticationContext(actionContext, Thread.CurrentPrincipal);
            var metadata = new FilterMetadata
            {
                ControllerType = typeof(TestController),
                FilterScope = FilterScope.Action,
                MethodInfo = methodInfo
            };
            var wrapper = new AuthenticationFilterWrapper(metadata);

            wrapper.OnAuthenticate(context);
            Assert.Equal(1, activationCount);
        }
    }
}