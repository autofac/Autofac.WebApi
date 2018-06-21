using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Autofac.Integration.WebApi.Test.TestTypes;
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
        public void ReturnsCorrectMetadataKey()
        {
            var wrapper = new ActionFilterWrapper(new FilterMetadata());
            Assert.Equal(AutofacWebApiFilterProvider.ActionFilterMetadataKey, wrapper.MetadataKey);
        }

        [Fact]
        public async void WrapperResolvesActionFilterFromDependencyScope()
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
            var controllerContext = CreateControllerContext(resolver);
            var methodInfo = typeof(TestController).GetMethod("Get");
            var actionDescriptor = CreateActionDescriptor(methodInfo);
            var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            var httpActionExecutedContext = new HttpActionExecutedContext(actionContext, null);
            var metadata = new FilterMetadata
            {
                ControllerType = typeof(TestController),
                FilterScope = FilterScope.Action,
                MethodInfo = methodInfo
            };
            var wrapper = new ActionFilterWrapper(metadata);

            await wrapper.OnActionExecutingAsync(actionContext, CancellationToken.None);
            Assert.Equal(1, activationCount);

            await wrapper.OnActionExecutedAsync(httpActionExecutedContext, CancellationToken.None);
            Assert.Equal(1, activationCount);
        }

        [Fact]
        public async void RunsFiltersInCorrectOrder()
        {
            // Issue #16: Filters need to run 1, 2, 3 in Executing but 3, 2, 1 in Executed.
            var builder = new ContainerBuilder();
            var order = new List<string>();

            builder.Register(ctx => new DelegatingLogger(s => order.Add(s)))
                .As<ILogger>()
                .SingleInstance();
            builder.RegisterType<TestActionFilter>()
                .AsWebApiActionFilterFor<TestController>(c => c.Get())
                .InstancePerRequest();
            builder.RegisterType<TestActionFilter2>()
                .AsWebApiActionFilterFor<TestController>(c => c.Get())
                .InstancePerRequest();

            var container = builder.Build();

            var resolver = new AutofacWebApiDependencyResolver(container);
            var controllerContext = CreateControllerContext(resolver);
            var methodInfo = typeof(TestController).GetMethod("Get");
            var actionDescriptor = CreateActionDescriptor(methodInfo);
            var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            var httpActionExecutedContext = new HttpActionExecutedContext(actionContext, null);
            var metadata = new FilterMetadata
            {
                ControllerType = typeof(TestController),
                FilterScope = FilterScope.Action,
                MethodInfo = methodInfo
            };
            var wrapper = new ActionFilterWrapper(metadata);

            await wrapper.OnActionExecutingAsync(actionContext, CancellationToken.None);
            await wrapper.OnActionExecutedAsync(httpActionExecutedContext, CancellationToken.None);
            Assert.Equal("TestActionFilter2.OnActionExecutingAsync", order[0]);
            Assert.Equal("TestActionFilter.OnActionExecutingAsync", order[1]);
            Assert.Equal("TestActionFilter.OnActionExecutedAsync", order[2]);
            Assert.Equal("TestActionFilter2.OnActionExecutedAsync", order[3]);
        }

        private static HttpActionDescriptor CreateActionDescriptor(MethodInfo methodInfo)
        {
            var controllerDescriptor = new HttpControllerDescriptor { ControllerType = methodInfo.DeclaringType };
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
            return actionDescriptor;
        }

        private static HttpControllerContext CreateControllerContext(AutofacWebApiDependencyResolver resolver)
        {
            var configuration = new HttpConfiguration { DependencyResolver = resolver };
            var requestMessage = new HttpRequestMessage();
            requestMessage.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, configuration);
            var controllerContext = new HttpControllerContext { Request = requestMessage };
            return controllerContext;
        }
    }
}