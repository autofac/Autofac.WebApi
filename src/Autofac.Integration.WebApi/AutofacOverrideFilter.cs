// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Allows other filters to be overriden at the control and action level.
    /// </summary>
    internal class AutofacOverrideFilter : IOverrideFilter
    {
        public AutofacOverrideFilter(Type filtersToOverride)
        {
            FiltersToOverride = filtersToOverride;
        }

        public bool AllowMultiple
        {
            get { return false; }
        }

        public Type FiltersToOverride
        {
            get;
            private set;
        }
    }
}
