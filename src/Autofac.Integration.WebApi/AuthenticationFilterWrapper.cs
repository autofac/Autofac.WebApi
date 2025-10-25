// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net.Http;
using System.Web.Http.Filters;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi;

/// <summary>
/// Resolves a filter for the specified metadata for each controller request.
/// </summary>
internal class AuthenticationFilterWrapper : IAuthenticationFilter, IAutofacAuthenticationFilter
{
    private readonly HashSet<FilterMetadata> _allFilters;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationFilterWrapper"/> class.
    /// </summary>
    /// <param name="filterMetadata">The filter metadata.</param>
    public AuthenticationFilterWrapper(HashSet<FilterMetadata> filterMetadata)
    {
        _allFilters = filterMetadata ?? throw new ArgumentNullException(nameof(filterMetadata));
    }

    /// <inheritdoc/>
    bool IFilter.AllowMultiple
    {
        get { return true; }
    }

    /// <inheritdoc/>
    public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var dependencyScope = context.Request.GetDependencyScope();
        var lifetimeScope = dependencyScope.GetRequestLifetimeScope();
        if (lifetimeScope == null)
        {
            return;
        }

        var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacAuthenticationFilter>>>>();

        foreach (var filter in filters.Where(FilterMatchesMetadata))
        {
            await filter.Value.Value.AuthenticateAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var dependencyScope = context.Request.GetDependencyScope();
        var lifetimeScope = dependencyScope.GetRequestLifetimeScope();
        if (lifetimeScope == null)
        {
            return;
        }

        var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacAuthenticationFilter>>>>();

        foreach (var filter in filters.Where(FilterMatchesMetadata))
        {
            await filter.Value.Value.ChallengeAsync(context, cancellationToken).ConfigureAwait(false);
        }
    }

    private bool FilterMatchesMetadata(Meta<Lazy<IAutofacAuthenticationFilter>> filter)
    {
        var metadata = filter.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var metadataAsObject)
            ? metadataAsObject as FilterMetadata
            : null;

        return metadata != null && _allFilters.Contains(metadata);
    }
}
