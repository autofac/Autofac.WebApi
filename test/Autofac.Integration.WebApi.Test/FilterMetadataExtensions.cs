﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

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
        /// <param name="registration">The registration for which metadata is being retrieved.</param>
        /// <param name="filterMeta">The metadata retrieved from the registration.</param>
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
