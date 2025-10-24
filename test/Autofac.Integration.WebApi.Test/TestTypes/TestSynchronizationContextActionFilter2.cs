// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test.TestTypes;

/// <summary>
/// An action filter that records the synchronization context before asynchronous continuations using
/// ConfigureAwait(false) to confirm the custom context flows through Autofac execution pipelines.
/// </summary>
public class TestSynchronizationContextActionFilter2 : IAutofacActionFilter
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
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);

        // Context changed, but that shouldn't affect the execution pipeline.
    }

    /// <inheritdoc />
    public async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
    {
        // Record the context before yielding to asynchronous work.
        _records.Add(SynchronizationContext.Current);

        // A minimal asynchronous delay ensures the continuation resumes and captures the context.
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);

        // Context changed, but that shouldn't affect the execution pipeline.
    }
}
