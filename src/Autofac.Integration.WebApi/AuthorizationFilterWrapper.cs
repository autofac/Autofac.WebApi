// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Resolves a filter for the specified metadata for each controller request.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Derived attribute adds filter override support")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    internal class AuthorizationFilterWrapper : AuthorizationFilterAttribute, IAutofacAuthorizationFilter
    {
        private readonly HashSet<FilterMetadata> _allFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationFilterWrapper"/> class.
        /// </summary>
        /// <param name="filterMetadata">The filter metadata.</param>
        public AuthorizationFilterWrapper(HashSet<FilterMetadata> filterMetadata)
        {
            _allFilters = filterMetadata ?? throw new ArgumentNullException(nameof(filterMetadata));
        }

        /// <summary>
        /// Gets the filter metadata.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> with the filter metadata the
        /// attribute was initialized with.
        /// </returns>
        public IEnumerable<FilterMetadata> FilterMetadata => _allFilters.AsEnumerable();

        /// <summary>
        /// Called when a process requests authorization.
        /// </summary>
        /// <param name="actionContext">The context for the action.</param>
        /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="actionContext" /> is <see langword="null" />.
        /// </exception>
        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var dependencyScope = actionContext.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacAuthorizationFilter>>>>();

            foreach (var filter in filters.Where(FilterMatchesMetadata))
            {
                await filter.Value.Value.OnAuthorizationAsync(actionContext, cancellationToken).ConfigureAwait(false);
            }
        }

        private bool FilterMatchesMetadata(Meta<Lazy<IAutofacAuthorizationFilter>> filter)
        {
            var metadata = filter.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var metadataAsObject)
                ? metadataAsObject as FilterMetadata
                : null;

            return _allFilters.Contains(metadata);
        }
    }
}
