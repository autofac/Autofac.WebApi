// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test;

public class AuthorizationFilterWrapperFixture
{
    [Fact]
    public void RequiresFilterMetadata()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new AuthorizationFilterWrapper(null));
        Assert.Equal("filterMetadata", exception.ParamName);
    }

    [Fact]
    public async Task WrapperResolvesAuthorizationFilterFromDependencyScope()
    {
        var builder = new ContainerBuilder();
        builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
        var activationCount = 0;
        builder.Register<IAutofacAuthorizationFilter>(c => new TestAuthorizationFilter(c.Resolve<ILogger>()))
            .AsWebApiAuthorizationFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .OnActivated(e => activationCount++)
            .GetMetadata(out var filterMetadata);
        var container = builder.Build();

        var resolver = new AutofacWebApiDependencyResolver(container);
        var configuration = new HttpConfiguration { DependencyResolver = resolver };
        var requestMessage = new HttpRequestMessage();
        requestMessage.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, configuration);
        var controllerContext = new HttpControllerContext { Request = requestMessage };
        var controllerDescriptor = new HttpControllerDescriptor { ControllerType = typeof(TestController) };
        var methodInfo = typeof(TestController).GetMethod("Get");
        var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);
        var actionContext = new HttpActionContext(controllerContext, actionDescriptor);

        var wrapper = new AuthorizationFilterWrapper(filterMetadata.ToSingleFilterHashSet());

        await wrapper.OnAuthorizationAsync(actionContext, CancellationToken.None);
        Assert.Equal(1, activationCount);
    }
}
