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
    internal class ActionFilterWrapper : ActionFilterAttribute, IAutofacActionFilter
    {
        private readonly HashSet<FilterMetadata> _allFilters;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionFilterWrapper"/> class.
        /// </summary>
        /// <param name="filterMetadata">The collection of filter metadata blocks that this wrapper should run.</param>
        public ActionFilterWrapper(HashSet<FilterMetadata> filterMetadata)
        {
            if (filterMetadata == null)
            {
                throw new ArgumentNullException(nameof(filterMetadata));
            }

            _allFilters = filterMetadata;
        }

        /// <summary>
        /// Occurs after the action method is invoked.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="actionExecutedContext" /> is <see langword="null" />.
        /// </exception>
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext == null)
            {
                throw new ArgumentNullException(nameof(actionExecutedContext));
            }

            var dependencyScope = actionExecutedContext.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacActionFilter>>>>();

            // Issue #16: OnActionExecuted needs to happen in the opposite order of OnActionExecuting.
            foreach (var filter in filters.Where(this.FilterMatchesMetadata).Reverse())
            {
                await filter.Value.Value.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
            }
        }

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The context for the action.</param>
        /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="actionContext" /> is <see langword="null" />.
        /// </exception>
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            var dependencyScope = actionContext.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacActionFilter>>>>();

            // Need to know the set of filters that we've executed so far, so
            // we can go back through them.
            var executedFilters = new List<IAutofacActionFilter>();

            // Issue #16: OnActionExecuted needs to happen in the opposite order of OnActionExecuting.
            foreach (var filter in filters.Where(this.FilterMatchesMetadata))
            {
                await filter.Value.Value.OnActionExecutingAsync(actionContext, cancellationToken);

                // Issue #30: If an actionfilter sets a response, it should prevent the others from running
                //            their own OnActionExecuting methods, and call OnActionExecuted on already-run
                //            filters.
                //            The OnActionExecutedAsync method of the wrapper will never fire if there
                //            is a response set, so we must call the OnActionExecutedAsync of prior filters
                //            ourselves.
                if (actionContext.Response != null)
                {
                    await ExecuteManualOnActionExecutedAsync(executedFilters.AsEnumerable().Reverse(), actionContext, cancellationToken);
                    break;
                }

                executedFilters.Add(filter.Value.Value);
            }
        }

        /// <summary>
        /// Method to manually invoke OnActionExecutedAsync outside of the filter wrapper's own
        /// OnActionExecuted async method.
        /// </summary>
        private async Task ExecuteManualOnActionExecutedAsync(
            IEnumerable<IAutofacActionFilter> filters,
            HttpActionContext actionContext,
            CancellationToken cancellationToken)
        {
            HttpActionExecutedContext localExecutedContext = null;

            foreach (var alreadyRanFilter in filters)
            {
                if (localExecutedContext == null)
                {
                    localExecutedContext = new HttpActionExecutedContext
                    {
                        ActionContext = actionContext,
                        Response = actionContext.Response
                    };
                }

                await alreadyRanFilter.OnActionExecutedAsync(localExecutedContext, cancellationToken);
            }
        }

        private bool FilterMatchesMetadata(Meta<Lazy<IAutofacActionFilter>> filter)
        {
            var metadata = filter.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var metadataAsObject)
                ? metadataAsObject as FilterMetadata
                : null;

            return _allFilters.Contains(metadata);
        }
    }
}