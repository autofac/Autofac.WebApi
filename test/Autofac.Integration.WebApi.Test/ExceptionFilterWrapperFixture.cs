// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Autofac.Integration.WebApi.Test.TestTypes;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class ExceptionFilterWrapperFixture
    {
        [Fact]
        public void RequiresFilterMetadata()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ExceptionFilterWrapper(null));
            Assert.Equal("filterMetadata", exception.ParamName);
        }

        [Fact]
        public async void WrapperResolvesExceptionFilterFromDependencyScope()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            var activationCount = 0;
            builder.Register<IAutofacExceptionFilter>(c => new TestExceptionFilter(c.Resolve<ILogger>()))
                .AsWebApiExceptionFilterFor<TestController>(c => c.Get())
                .InstancePerRequest()
                .OnActivated(e => activationCount++)
                .GetMetadata(out var filterMetadata);
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
            var actionExecutedContext = new HttpActionExecutedContext(actionContext, null);
            var wrapper = new ExceptionFilterWrapper(filterMetadata.ToSingleFilterHashSet());

            await wrapper.OnExceptionAsync(actionExecutedContext, CancellationToken.None);
            Assert.Equal(1, activationCount);
        }
    }
}
