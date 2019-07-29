// This software is part of the Autofac IoC container
// Copyright © 2012 Autofac Contributors
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