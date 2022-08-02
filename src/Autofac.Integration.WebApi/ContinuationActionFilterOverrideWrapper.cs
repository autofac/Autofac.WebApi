// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi;

/// <summary>
/// Resolves a filter override for the specified metadata for each controller request.
/// </summary>
internal sealed class ContinuationActionFilterOverrideWrapper : ContinuationActionFilterWrapper, IOverrideFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContinuationActionFilterOverrideWrapper"/> class.
    /// </summary>
    /// <param name="filterMetadata">The filter metadata.</param>
    public ContinuationActionFilterOverrideWrapper(HashSet<FilterMetadata> filterMetadata)
        : base(filterMetadata)
    {
    }

    /// <summary>
    /// Gets the filters to override.
    /// </summary>
    public Type FiltersToOverride
    {
        get { return typeof(IActionFilter); }
    }
}
