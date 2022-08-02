// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi;

/// <summary>
/// An authentication filter that will be created for each controller request.
/// </summary>
public interface IAutofacAuthenticationFilter
{
    /// <summary>
    /// Called when a request requires authentication.
    /// </summary>
    /// <param name="context">The context for the authentication.</param>
    /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
    Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Called when an authentication challenge is required.
    /// </summary>
    /// <param name="context">The context for the authentication challenge.</param>
    /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
    Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken);
}
