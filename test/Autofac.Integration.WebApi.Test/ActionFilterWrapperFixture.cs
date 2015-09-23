using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class ActionFilterWrapperFixture
    {
        [Fact]
        public void RequiresFilterMetadata()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ActionFilterWrapper(null));
            Assert.Equal("filterMetadata", exception.ParamName);
        }

        [Fact]
        public void WrapperResolvesActionFilterFromDependencyScope()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            var activationCount = 0;
            builder.Register<IAutofacActionFilter>(c => new TestActionFilter(c.Resolve<ILogger>()))
                .AsWebApiActionFilterFor<TestController>(c => c.Get())
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
            var httpActionContext = new HttpActionContext(contollerContext, actionDescriptor);
            var actionContext = new HttpActionContext(contollerContext, actionDescriptor);
            var httpActionExecutedContext = new HttpActionExecutedContext(actionContext, null);
            var metadata = new FilterMetadata
            {
                ControllerType = typeof(TestController),
                FilterScope = FilterScope.Action,
                MethodInfo = methodInfo
            };
            var wrapper = new ActionFilterWrapper(metadata);

            wrapper.OnActionExecuting(httpActionContext);
            Assert.Equal(1, activationCount);

            wrapper.OnActionExecuted(httpActionExecutedContext);
            Assert.Equal(1, activationCount);
        }

        [Fact]
        public void ReturnsCorrectMetadataKey()
        {
            var wrapper = new ActionFilterWrapper(new FilterMetadata());
            Assert.Equal(AutofacWebApiFilterProvider.ActionFilterMetadataKey, wrapper.MetadataKey);
        }
    }
}