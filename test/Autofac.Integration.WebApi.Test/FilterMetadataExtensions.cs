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

using System.Collections.Generic;
using Autofac.Builder;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    internal static class FilterMetadataExtensions
    {
        public static HashSet<FilterMetadata> ToSingleFilterHashSet(this FilterMetadata metadata)
        {
            return new HashSet<FilterMetadata> { metadata };
        }

        /// <summary>
        /// Retrieve or create filter metadata. We want to maintain the fluent flow when we change
        /// registration metadata so we'll do that here.
        /// </summary>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> GetMetadata(
            this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
            out FilterMetadata filterMeta)
        {
            Assert.True(registration.RegistrationData.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var filterDataObj));

            filterMeta = (FilterMetadata)filterDataObj;

            Assert.NotNull(filterMeta);

            return registration;
        }
    }
}