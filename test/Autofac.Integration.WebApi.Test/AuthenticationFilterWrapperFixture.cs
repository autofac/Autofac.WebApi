// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using System.Web.Http.Results;
using Autofac.Integration.WebApi.Test.TestTypes;
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
        public async void WrapperExecutesAuthenticationFilters()
        {
            var builder = new ContainerBuilder();
            var output = new List<string>();
            builder.Register<ILogger>(c => new DelegatingLogger(m => output.Add(m))).InstancePerDependency();
            builder.Register<IAutofacAuthenticationFilter>(c => new TestAuthenticationFilter(c.Resolve<ILogger>()))
                .AsWebApiAuthenticationFilterFor<TestController>(c => c.Get())
                .InstancePerRequest()
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
            var context = new HttpAuthenticationContext(actionContext, Thread.CurrentPrincipal);

            var challengeContext = new HttpAuthenticationChallengeContext(actionContext, new StatusCodeResult(HttpStatusCode.Forbidden, requestMessage));

            var wrapper = new AuthenticationFilterWrapper(filterMetadata.ToSingleFilterHashSet());

            await wrapper.AuthenticateAsync(context, CancellationToken.None);

            await wrapper.ChallengeAsync(challengeContext, CancellationToken.None);

            Assert.Equal(nameof(wrapper.AuthenticateAsync), output[0]);
            Assert.Equal(nameof(wrapper.ChallengeAsync), output[1]);
        }

        [Fact]
        public async void WrapperResolvesAuthenticationFilterFromDependencyScope()
        {
            var builder = new ContainerBuilder();
            builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
            var activationCount = 0;
            builder.Register<IAutofacAuthenticationFilter>(c => new TestAuthenticationFilter(c.Resolve<ILogger>()))
                .AsWebApiAuthenticationFilterFor<TestController>(c => c.Get())
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
            var context = new HttpAuthenticationContext(actionContext, Thread.CurrentPrincipal);

            var challengeContext = new HttpAuthenticationChallengeContext(actionContext, new StatusCodeResult(HttpStatusCode.Forbidden, requestMessage));

            var wrapper = new AuthenticationFilterWrapper(filterMetadata.ToSingleFilterHashSet());

            await wrapper.AuthenticateAsync(context, CancellationToken.None);

            await wrapper.ChallengeAsync(challengeContext, CancellationToken.None);

            Assert.Equal(1, activationCount);
        }
    }
}
