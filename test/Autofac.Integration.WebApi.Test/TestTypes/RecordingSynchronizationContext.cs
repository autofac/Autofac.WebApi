// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.WebApi.Test.TestTypes;

/// <summary>
/// A lightweight synchronization context used in tests to execute posted work immediately
/// while ensuring the custom context remains current across callbacks.
/// </summary>
public class RecordingSynchronizationContext : SynchronizationContext
{
    /// <summary>
    /// Dispatches asynchronous work onto the test synchronization context and ensures the
    /// context is restored afterwards.
    /// </summary>
    /// <param name="d">The callback delegate to execute.</param>
    /// <param name="state">The optional state to provide to the callback.</param>
    public override void Post(SendOrPostCallback d, object state)
    {
        if (d is null)
        {
            throw new ArgumentNullException(nameof(d));
        }

        var previous = Current;
        try
        {
            // Ensure continuations observe the custom context while the callback runs.
            SetSynchronizationContext(this);
            d(state);
        }
        finally
        {
            SetSynchronizationContext(previous);
        }
    }

    /// <summary>
    /// Dispatches synchronous work and mirrors the behavior of <see cref="Post"/> by
    /// restoring the original context when complete.
    /// </summary>
    /// <param name="d">The callback delegate to execute.</param>
    /// <param name="state">The optional state to provide to the callback.</param>
    public override void Send(SendOrPostCallback d, object state)
    {
        if (d is null)
        {
            throw new ArgumentNullException(nameof(d));
        }

        var previous = Current;
        try
        {
            SetSynchronizationContext(this);
            d(state);
        }
        finally
        {
            SetSynchronizationContext(previous);
        }
    }
}
