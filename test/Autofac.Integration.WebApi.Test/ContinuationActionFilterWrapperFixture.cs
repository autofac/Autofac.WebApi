// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Transactions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test;

public class ContinuationActionFilterWrapperFixture
{
    [Fact]
    public void RequiresFilterMetadata()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ContinuationActionFilterWrapper(null));
        Assert.Equal("filterMetadata", exception.ParamName);
    }

    [Fact]
    public async Task WrapperResolvesActionFilterFromDependencyScope()
    {
        var builder = new ContainerBuilder();
        builder.Register<ILogger>(c => new Logger()).InstancePerDependency();
        using var configuration = new HttpConfiguration();
        builder.RegisterWebApiFilterProvider(configuration);
        var activationCount = 0;
        builder.Register<IAutofacActionFilter>(c => new TestActionFilter(c.Resolve<ILogger>()))
               .AsWebApiActionFilterFor<TestController>(c => c.Get())
               .InstancePerRequest()
               .OnActivated(e => activationCount++)
               .GetMetadata(out var filterMetadata);

        var container = builder.Build();

        var resolver = new AutofacWebApiDependencyResolver(container);
        var controllerContext = CreateControllerContext(resolver);
        var methodInfo = typeof(TestController).GetMethod("Get");
        var actionDescriptor = CreateActionDescriptor(methodInfo);
        var actionContext = new HttpActionContext(controllerContext, actionDescriptor);

        var wrapper = new ContinuationActionFilterWrapper(filterMetadata.ToSingleFilterHashSet());

        await wrapper.ExecuteActionFilterAsync(actionContext, CancellationToken.None, () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        Assert.Equal(1, activationCount);
    }

    [Fact]
    public async Task RunsFiltersInCorrectOrder()
    {
        // Issue #16: Filters need to run 1, 2, 3 in Executing but 3, 2, 1 in Executed.
        var builder = new ContainerBuilder();
        var order = new List<string>();

        builder.Register(ctx => new DelegatingLogger(s => order.Add(s)))
            .As<ILogger>()
            .SingleInstance();
        builder.RegisterType<TestActionFilter>()
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testFilter1Meta);
        builder.RegisterType<TestActionFilter2>()
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testFilter2Meta);

        using var configuration = new HttpConfiguration();

        builder.RegisterWebApiFilterProvider(configuration);

        var container = builder.Build();

        var resolver = new AutofacWebApiDependencyResolver(container);
        var controllerContext = CreateControllerContext(resolver);
        var methodInfo = typeof(TestController).GetMethod("Get");
        var actionDescriptor = CreateActionDescriptor(methodInfo);
        var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
        var wrapper = new ContinuationActionFilterWrapper(new HashSet<FilterMetadata>
        {
            testFilter1Meta,
            testFilter2Meta,
        });

        await wrapper.ExecuteActionFilterAsync(
            actionContext,
            CancellationToken.None,
            () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        Assert.Equal("TestActionFilter.OnActionExecutingAsync", order[0]);
        Assert.Equal("TestActionFilter2.OnActionExecutingAsync", order[1]);
        Assert.Equal("TestActionFilter2.OnActionExecutedAsync", order[2]);
        Assert.Equal("TestActionFilter.OnActionExecutedAsync", order[3]);
    }

    [Fact]
    public async Task StopsIfFilterOnExecutingSetsResponse()
    {
        // Issue #30.
        // The filter behavior if a response is set should be as follows, to
        // mirror the functionality of filters in the normal IActionFilter implementations.
        //
        // If a filter sets the response:
        // - OnActionExecutingAsync from subsequent calls should not be invoked.
        // - Its own OnActionExecutedAsync should not be invoked.
        // - OnActionExecutedAsync for prior filters should still be invoked.
        var builder = new ContainerBuilder();
        var order = new List<string>();

        builder.Register(ctx => new DelegatingLogger(s => order.Add(s)))
            .As<ILogger>()
            .SingleInstance();
        builder.RegisterType<TestActionFilter>()
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testActionFilterMetadata);
        builder.RegisterType<TestActionFilter2>()
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testActionFilter2Metadata);
        builder.RegisterType<TestActionFilterWithResponse>()
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testActionFilterWithResponseMetadata);
        builder.RegisterType<TestActionFilter3>()
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testActionFilter3Metadata);

        using var configuration = new HttpConfiguration();

        builder.RegisterWebApiFilterProvider(configuration);

        var container = builder.Build();

        var resolver = new AutofacWebApiDependencyResolver(container);
        var controllerContext = CreateControllerContext(resolver);
        var methodInfo = typeof(TestController).GetMethod("Get");
        var actionDescriptor = CreateActionDescriptor(methodInfo);
        var actionContext = new HttpActionContext(controllerContext, actionDescriptor);

        var wrapper = new ContinuationActionFilterWrapper(new HashSet<FilterMetadata>
        {
            testActionFilterMetadata,
            testActionFilterWithResponseMetadata,
            testActionFilter2Metadata,
            testActionFilter3Metadata,
        });

        await wrapper.ExecuteActionFilterAsync(
            actionContext,
            CancellationToken.None,
            () => throw new InvalidOperationException("Should never reach here because a filter set the response."));

        Assert.Equal("TestActionFilter.OnActionExecutingAsync", order[0]);
        Assert.Equal("TestActionFilter2.OnActionExecutingAsync", order[1]);
        Assert.Equal("TestActionFilterWithResponse.OnActionExecutingAsync", order[2]);
        Assert.Equal("TestActionFilter2.OnActionExecutedAsync", order[3]);
        Assert.Equal("TestActionFilter.OnActionExecutedAsync", order[4]);
        Assert.Equal(5, order.Count);
    }

    [Fact]
    public void TransactionScopePreservedBetweenStandardFilters()
    {
        // Issue #34 - Async/await context lost between filters.
        var builder = new ContainerBuilder();

        TransactionScope scope = null;

        // Since Autofac 4.0.1 filters will resolve in registration order.
        builder.Register(s => new TestCallbackActionFilter(
                () =>
                    scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled),
                () =>
                {
                    Assert.NotNull(Transaction.Current);
                    scope.Dispose();
                    Assert.Null(Transaction.Current);
                }))
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testFilter1Meta);

        builder.Register(s => new TestCallbackActionFilter(
                () =>
                    Assert.NotNull(Transaction.Current),
                () =>
                    Assert.NotNull(Transaction.Current)))
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testFilter2Meta);

        using var configuration = new HttpConfiguration();

        builder.RegisterWebApiFilterProvider(configuration);

        var container = builder.Build();

        var resolver = new AutofacWebApiDependencyResolver(container);
        var controllerContext = CreateControllerContext(resolver);
        var methodInfo = typeof(TestController).GetMethod("Get");
        var actionDescriptor = CreateActionDescriptor(methodInfo);
        var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
        var wrapper = new ContinuationActionFilterWrapper(new HashSet<FilterMetadata>
        {
            testFilter1Meta,
            testFilter2Meta,
        });

        wrapper.ExecuteActionFilterAsync(
            actionContext,
            CancellationToken.None,
            () =>
            {
                Assert.NotNull(Transaction.Current);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }).Wait();
    }

    [Fact]
    public void TransactionScopePreservedBetweenContinuationFilters()
    {
        // Issue #34 - Async/await context lost between filters.
        var builder = new ContainerBuilder();

        // Since Autofac 4.0.1 filters will resolve in registration order.
        builder.Register(s => new TestContinuationActionFilterWithTransactionScope(
                () =>
                    Assert.Null(Transaction.Current),
                () =>
                    Assert.Null(Transaction.Current)))
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testFilter1Meta);
        builder.Register(s => new TestContinuationActionFilter(
                () =>
                    Assert.NotNull(Transaction.Current),
                () =>
                    Assert.NotNull(Transaction.Current)))
            .AsWebApiActionFilterFor<TestController>(c => c.Get())
            .InstancePerRequest()
            .GetMetadata(out var testFilter2Meta);

        using var configuration = new HttpConfiguration();

        builder.RegisterWebApiFilterProvider(configuration);

        var container = builder.Build();

        var resolver = new AutofacWebApiDependencyResolver(container);
        var controllerContext = CreateControllerContext(resolver);
        var methodInfo = typeof(TestController).GetMethod("Get");
        var actionDescriptor = CreateActionDescriptor(methodInfo);
        var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
        var wrapper = new ContinuationActionFilterWrapper(new HashSet<FilterMetadata>
        {
            testFilter1Meta,
            testFilter2Meta,
        });

        wrapper.ExecuteActionFilterAsync(
            actionContext,
            CancellationToken.None,
            () =>
            {
                Assert.NotNull(Transaction.Current);
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            }).Wait();
    }

    /// <summary>
    /// Verifies that a custom synchronization context flows through Autofac action filter execution
    /// including the action continuation invoked by <see cref="Autofac.Integration.WebApi.ContinuationActionFilterWrapper"/>.
    /// </summary>
    [Fact]
    [SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "We're managing the SynchronizationContext in this test")]

    public async Task SynchronizationContextPreservedAcrossFilterExecutionAsync()
    {
        // Issue #72 - HttpContext.Current becomes null when an async OnActionExecutingAsync Web API filter runs.
        var builder = new ContainerBuilder();

        TestSynchronizationContextActionFilter filterInstance = null;
        TestSynchronizationContextActionFilter2 filterInstance2 = null;

        builder.RegisterType<TestSynchronizationContextActionFilter>()
            .AsWebApiActionFilterFor<TestController>(c => c.GetAsync(default))
            .InstancePerRequest()
            .OnActivated(e => filterInstance = (TestSynchronizationContextActionFilter)e.Instance)
            .GetMetadata(out var filterMetadata);

        builder.RegisterType<TestSynchronizationContextActionFilter2>()
            .AsWebApiActionFilterFor<TestController>(c => c.GetAsync(default))
            .InstancePerRequest()
            .OnActivated(e => filterInstance2 = (TestSynchronizationContextActionFilter2)e.Instance)
            .GetMetadata(out var filterMetadata2);

        using var configuration = new HttpConfiguration();

        builder.RegisterWebApiFilterProvider(configuration);

        var container = builder.Build();

        var resolver = new AutofacWebApiDependencyResolver(container);
        var controllerContext = CreateControllerContext(resolver);
        var methodInfo = typeof(TestController).GetMethod(nameof(TestController.GetAsync));
        var actionDescriptor = CreateActionDescriptor(methodInfo);
        var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
        var wrapper = new ContinuationActionFilterWrapper(filterMetadata.ToSingleFilterHashSet());
        var wrapper2 = new ContinuationActionFilterWrapper(filterMetadata2.ToSingleFilterHashSet());

        var recordedContinuationContexts = new List<SynchronizationContext>();
        var customContext = new RecordingSynchronizationContext();
        var originalContext = SynchronizationContext.Current;

        // Install the custom synchronization context so both the filter and the continuation can observe it.
        SynchronizationContext.SetSynchronizationContext(customContext);

        try
        {
            // TestSynchronizationContextActionFilter doesn't use ConfigureAwait(false)
            await wrapper.ExecuteActionFilterAsync(
                actionContext,
                CancellationToken.None,
                async () =>
                {
                    // Record the context before and after an asynchronous boundary inside the continuation.
                    recordedContinuationContexts.Add(SynchronizationContext.Current);
                    await Task.Delay(1);
                    recordedContinuationContexts.Add(SynchronizationContext.Current);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            // TestSynchronizationContextActionFilter2 uses ConfigureAwait(false), but the context should still flow.
            await wrapper2.ExecuteActionFilterAsync(
                actionContext,
                CancellationToken.None,
                async () =>
                {
                    // Record the context before and after an asynchronous boundary inside the continuation.
                    recordedContinuationContexts.Add(SynchronizationContext.Current);
                    await Task.Delay(1);
                    recordedContinuationContexts.Add(SynchronizationContext.Current);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });
        }
        finally
        {
            // Always restore the original context to avoid polluting later tests.
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }

        Assert.NotNull(filterInstance);
        Assert.NotNull(filterInstance2);
        Assert.All(
            filterInstance.Records.Concat(filterInstance2.Records).Concat(recordedContinuationContexts),
            context => Assert.Same(customContext, context));
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
