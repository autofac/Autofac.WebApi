// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
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
using System.Web.Http.Filters;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Resolves a filter for the specified metadata for each controller request.
    /// </summary>
    internal class AuthenticationFilterWrapper : IAuthenticationFilter, IAutofacAuthenticationFilter, IFilterWrapper
    {
        private readonly FilterMetadata _filterMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationFilterWrapper"/> class.
        /// </summary>
        /// <param name="filterMetadata">The filter metadata.</param>
        public AuthenticationFilterWrapper(FilterMetadata filterMetadata)
        {
            if (filterMetadata == null)
            {
                throw new ArgumentNullException(nameof(filterMetadata));
            }

            this._filterMetadata = filterMetadata;
        }

        /// <summary>
        /// Gets the metadata key used to retrieve the filter metadata.
        /// </summary>
        public virtual string MetadataKey
        {
            get { return AutofacWebApiFilterProvider.AuthenticationFilterMetadataKey; }
        }

        bool IFilter.AllowMultiple
        {
            get { return true; }
        }

        public async Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var dependencyScope = context.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacAuthenticationFilter>>>>();

            foreach (var filter in filters.Where(this.FilterMatchesMetadata))
            {
                await filter.Value.Value.AuthenticateAsync(context, cancellationToken);
            }
        }

        public async Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var dependencyScope = context.Request.GetDependencyScope();
            var lifetimeScope = dependencyScope.GetRequestLifetimeScope();

            var filters = lifetimeScope.Resolve<IEnumerable<Meta<Lazy<IAutofacAuthenticationFilter>>>>();

            foreach (var filter in filters.Where(this.FilterMatchesMetadata))
            {
                await filter.Value.Value.ChallengeAsync(context, cancellationToken);
            }
        }

        private bool FilterMatchesMetadata(Meta<Lazy<IAutofacAuthenticationFilter>> filter)
        {
            var metadata = filter.Metadata.TryGetValue(this.MetadataKey, out var metadataAsObject)
                ? metadataAsObject as FilterMetadata
                : null;

            return metadata != null
                   && metadata.ControllerType == this._filterMetadata.ControllerType
                   && metadata.FilterScope == this._filterMetadata.FilterScope
                   && metadata.MethodInfo == this._filterMetadata.MethodInfo;
        }
    }
}