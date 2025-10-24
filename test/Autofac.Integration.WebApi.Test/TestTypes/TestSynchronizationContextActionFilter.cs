// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test.TestTypes;

/// <summary>
/// An action filter that records the synchronization context before and after asynchronous
/// continuations to confirm the custom context flows through Autofac execution pipelines.
/// </summary>
[SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", Justification = "Test case where SynchronizationContext should be preserved in the filter")]
public class TestSynchronizationContextActionFilter : IAutofacActionFilter
{
    private readonly List<SynchronizationContext> _records = new();

    /// <summary>
    /// Gets the ordered collection of synchronization contexts recorded during filter execution.
    /// </summary>
    public IReadOnlyList<SynchronizationContext> Records => _records;

    /// <inheritdoc />
    public async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
    {
        // Record the context before yielding to asynchronous work.
        _records.Add(SynchronizationContext.Current);

        // A minimal asynchronous delay ensures the continuation resumes and captures the context.
        await Task.Delay(1, cancellationToken);

        // Record the context again after the await to validate it has not changed.
        _records.Add(SynchronizationContext.Current);
    }

    /// <inheritdoc />
    public async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
    {
        // Record the context before yielding to asynchronous work.
        _records.Add(SynchronizationContext.Current);

        // A minimal asynchronous delay ensures the continuation resumes and captures the context.
        await Task.Delay(1, cancellationToken);

        // Record the context again after the await to validate it has not changed.
        _records.Add(SynchronizationContext.Current);
    }
}
