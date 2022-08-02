// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.WebApi;

/// <summary>
/// A delegating handler that updates the current dependency scope
/// with the current <see cref="HttpRequestMessage"/>.
/// </summary>
internal class CurrentRequestHandler : DelegatingHandler
{
    /// <summary>
    /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
    /// </summary>
    /// <param name="request">The HTTP request message to send to the server.</param>
    /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
    /// <returns>
    /// Returns <see cref="System.Threading.Tasks.Task{T}" />. The task object representing the asynchronous operation.
    /// </returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        UpdateScopeWithHttpRequestMessage(request);

        return base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Updates the current dependency scope with current HTTP request message.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    internal static void UpdateScopeWithHttpRequestMessage(HttpRequestMessage request)
    {
        HttpRequestMessageProvider.Current = request;
    }
}
