// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi;

/// <summary>
/// Resolves a filter override for the specified metadata for each controller request.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
internal sealed class ExceptionFilterOverrideWrapperAttribute : ExceptionFilterWrapperAttribute, IOverrideFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionFilterOverrideWrapperAttribute"/> class.
    /// </summary>
    /// <param name="filterMetadata">The filter metadata.</param>
    public ExceptionFilterOverrideWrapperAttribute(HashSet<FilterMetadata> filterMetadata)
        : base(filterMetadata)
    {
    }

    /// <summary>
    /// Gets the filters to override.
    /// </summary>
    public Type FiltersToOverride
    {
        get
        {
            return typeof(IExceptionFilter);
        }
    }
}
