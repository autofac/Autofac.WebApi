// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace Autofac.Integration.WebApi;

/// <summary>
/// An action filter that will be created for each controller request, and
/// executes using continuations, so the async context is preserved.
/// </summary>
public interface IAutofacContinuationActionFilter
{
    /// <summary>
    /// The method called when the filter executes. The filter should call 'next' to
    /// continue processing the request.
    /// </summary>
    /// <param name="actionContext">The context of the current action.</param>
    /// <param name="cancellationToken">A cancellation token for the request.</param>
    /// <param name="next">The function to call that invokes the next filter in the chain.</param>
    /// <returns>The result of the pipeline on the request message.</returns>
    [SuppressMessage("Microsoft.CodeQuality", "CA1068", Justification = "Matching parameter order in IActionFilter.")]
    Task<HttpResponseMessage> ExecuteActionFilterAsync(
        HttpActionContext actionContext,
        CancellationToken cancellationToken,
        Func<Task<HttpResponseMessage>> next);
}
