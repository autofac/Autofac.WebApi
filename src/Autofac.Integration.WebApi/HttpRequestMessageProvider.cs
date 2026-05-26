// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Autofac.Integration.WebApi;

/// <summary>
/// Provider that holds the current <see cref="HttpRequestMessage"/> for injection as a dependency.
/// </summary>
internal static class HttpRequestMessageProvider
{
    private static readonly AsyncLocal<HttpRequestMessageHolder> _currentRequest = new();

    /// <summary>
    /// Gets or sets the current request message.
    /// </summary>
    /// <value>
    /// The <see cref="HttpRequestMessage"/> for the current/ongoing request.
    /// </value>
    internal static HttpRequestMessage? Current
    {
        get
        {
            return _currentRequest.Value?._message;
        }

        set
        {
            var holder = _currentRequest.Value;

            // Clear current HttpRequestMessage trapped in the AsyncLocals, as its done.
            holder?._message = null;

            if (value != null)
            {
                // Use an object indirection to hold the HttpRequestMessage in the AsyncLocal,
                // so it can be cleared in all ExecutionContexts when its cleared.
                _currentRequest.Value = new HttpRequestMessageHolder { _message = value };
            }
        }
    }

    private sealed class HttpRequestMessageHolder
    {
        [SuppressMessage("SA1401", "SA1401", Justification = "Field is only used during testing.")]
        public HttpRequestMessage? _message;
    }
}
