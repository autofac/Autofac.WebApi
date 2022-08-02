// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Controllers;

namespace Autofac.Integration.WebApi;

/// <summary>
/// An authorization filter that will be created for each controller request.
/// </summary>
public interface IAutofacAuthorizationFilter
{
    /// <summary>
    /// Called when a process requests authorization.
    /// </summary>
    /// <param name="actionContext">The context for the action.</param>
    /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
    Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken);
}
