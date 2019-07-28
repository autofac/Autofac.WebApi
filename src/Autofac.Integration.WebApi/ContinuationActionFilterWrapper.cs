// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

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
            if (filterMetadata == null)
            {
                throw new ArgumentNullException(nameof(filterMetadata));
            }

            _allFilters = filterMetadata;
        }

        private bool FilterMatchesMetadata(Meta<Lazy<IAutofacContinuationActionFilter>> filter)
        {
            var metadata = filter.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var metadataAsObject)
                ? metadataAsObject as FilterMetadata
                : null;

            return _allFilters.Contains(metadata);
        }

        public bool AllowMultiple { get; } = true;

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

            foreach (var filterStage in filters.Reverse().Where(FilterMatchesMetadata))
            {
                result = ChainContinuation(result, filterStage.Value.Value);
            }

            return result();
        }
    }
}