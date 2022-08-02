// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi;

/// <summary>
/// An exception filter that will be created for each controller request.
/// </summary>
public interface IAutofacExceptionFilter
{
    /// <summary>
    /// Called when an exception is thrown.
    /// </summary>
    /// <param name="actionExecutedContext">The context for the action.</param>
    /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
    Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken);
}
