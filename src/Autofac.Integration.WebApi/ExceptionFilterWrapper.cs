// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Resolves a filter for the specified metadata for each controller request.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "Derived attribute adds filter override support")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    internal class ExceptionFilterWrapper : ExceptionFilterAttribute, IAutofacExceptionFilter
    {
        private readonly HashSet<FilterMetadata> _allFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionFilterWrapper"/> class.
        /// </summary>
        /// <param name="filterMetadata">The filter metadata.</param>
        public ExceptionFilterWrapper(HashSet<FilterMetadata> filterMetadata)
        {
            this._allFilters = filterMetadata ?? throw new ArgumentNullException(nameof(filterMetadata));
        }

        /// <summary>
        /// Called when an exception is thrown.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="actionExecutedContext" /> is <see langword="null" />.
        /// </exception>
        public override async Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext == null)
            {
                throw new ArgumentNullException(nameof(actionExecutedContext));
            }

            var dependencyScope = actionExecutedContext.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacExceptionFilter>>>>();

            foreach (var filter in filters.Where(this.FilterMatchesMetadata))
            {
                await filter.Value.Value.OnExceptionAsync(actionExecutedContext, cancellationToken);
            }
        }

        private bool FilterMatchesMetadata(Meta<Lazy<IAutofacExceptionFilter>> filter)
        {
            var metadata = filter.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var metadataAsObject)
                ? metadataAsObject as FilterMetadata
                : null;

            return _allFilters.Contains(metadata);
        }
    }
}
