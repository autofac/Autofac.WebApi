// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Allows other filters to be overridden at the control and action level.
    /// </summary>
    internal class AutofacOverrideFilter : IOverrideFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacOverrideFilter"/> class.
        /// </summary>
        /// <param name="filtersToOverride">
        /// The <see cref="Type"/> of filters to override.
        /// </param>
        public AutofacOverrideFilter(Type filtersToOverride)
        {
            FiltersToOverride = filtersToOverride;
        }

        /// <inheritdoc/>
        public bool AllowMultiple
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the filter type to override.
        /// </summary>
        /// <value>
        /// A <see cref="Type"/> indicating the filter type to override.
        /// </value>
        public Type FiltersToOverride
        {
            get;
            private set;
        }
    }
}
