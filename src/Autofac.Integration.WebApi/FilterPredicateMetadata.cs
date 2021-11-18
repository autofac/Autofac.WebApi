// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Metadata block for an individual filter predicate.
    /// </summary>
    internal class FilterPredicateMetadata
    {
        /// <summary>
        /// Gets or sets the callback that determines if a filter matches the action descriptor.
        /// Returns true/false to include the filter or not.
        /// </summary>
        public Func<ILifetimeScope, HttpActionDescriptor, bool> Predicate { get; set; }

        /// <summary>
        /// Gets or sets the scope of the filter.
        /// </summary>
        /// <remarks>
        /// We need the scope of this filter registration so we can create the FilterInfo later.
        /// </remarks>
        public FilterScope Scope { get; set; }

        /// <summary>
        /// Gets or sets the filter category, used to group filters and control execution order.
        /// </summary>
        public AutofacFilterCategory FilterCategory { get; set; }
    }
}
