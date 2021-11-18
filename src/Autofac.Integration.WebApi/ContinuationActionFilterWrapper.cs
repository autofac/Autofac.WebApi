// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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
    internal class ContinuationActionFilterWrapper : IActionFilter, IAutofacContinuationActionFilter
    {
        private readonly HashSet<FilterMetadata> _allFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuationActionFilterWrapper"/> class.
        /// </summary>
        /// <param name="filterMetadata">The collection of filter metadata blocks that this wrapper should run.</param>
        public ContinuationActionFilterWrapper(HashSet<FilterMetadata> filterMetadata)
        {
            _allFilters = filterMetadata ?? throw new ArgumentNullException(nameof(filterMetadata));
        }

        /// <inheritdoc/>
        public bool AllowMultiple { get; } = true;

        /// <inheritdoc/>
        public Task<HttpResponseMessage> ExecuteActionFilterAsync(
            HttpActionContext actionContext,
            CancellationToken cancellationToken,
            Func<Task<HttpResponseMessage>> continuation)
        {
            var dependencyScope = actionContext.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacContinuationActionFilter>>>>();

            Func<Task<HttpResponseMessage>> result = continuation;

            Func<Task<HttpResponseMessage>> ChainContinuation(Func<Task<HttpResponseMessage>> next, IAutofacContinuationActionFilter innerFilter)
            {
                return () => innerFilter.ExecuteActionFilterAsync(actionContext, cancellationToken, next);
            }

            // We go backwards for the beginning of the set of filters, where
            // the last one invokes the provided continuation, the previous one invokes the last one, and so on,
            // until there's a callback that invokes the first filter.
            foreach (var filterStage in filters.Reverse().Where(FilterMatchesMetadata))
            {
                result = ChainContinuation(result, filterStage.Value.Value);
            }

            return result();
        }

        private bool FilterMatchesMetadata(Meta<Lazy<IAutofacContinuationActionFilter>> filter)
        {
            var metadata = filter.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var metadataAsObject)
                ? metadataAsObject as FilterMetadata
                : null;

            return _allFilters.Contains(metadata);
        }
    }
}
