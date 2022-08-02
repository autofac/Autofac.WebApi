// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Core.Lifetime;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// A filter provider for performing property injection on filter attributes.
    /// </summary>
    public class AutofacWebApiFilterProvider : IFilterProvider
    {
        private class FilterContext
        {
            public FilterContext(
                ILifetimeScope lifetimeScope,
                Type controllerType,
                List<FilterInfo> filters,
                Dictionary<AutofacFilterCategory, List<FilterPredicateMetadata>> addedFilters)
            {
                LifetimeScope = lifetimeScope;
                ControllerType = controllerType;
                Filters = filters;
                AddedFilters = addedFilters;
            }

            public ILifetimeScope LifetimeScope { get; }

            public Type ControllerType { get; }

            public List<FilterInfo> Filters { get; }

            public Dictionary<AutofacFilterCategory, List<FilterPredicateMetadata>> AddedFilters { get; }
        }

        private readonly ILifetimeScope _rootLifetimeScope;
        private readonly ActionDescriptorFilterProvider _filterProvider = new();

        /// <summary>
        /// Metadata key that holds Autofac filter data.
        /// </summary>
        internal const string FilterMetadataKey = "AutofacFilterData";

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacWebApiFilterProvider"/> class.
        /// </summary>
        /// <param name="lifetimeScope">
        /// The lifetime scope from which dependencies should be resolved. Generally
        /// this is the application-level container/root scope.
        /// </param>
        public AutofacWebApiFilterProvider(ILifetimeScope lifetimeScope)
        {
            _rootLifetimeScope = lifetimeScope;
        }

        /// <summary>
        /// Returns the collection of filters associated with <paramref name="actionDescriptor"/>.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        /// <returns>A collection of filters with instances property injected.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="configuration" /> is <see langword="null" />.
        /// </exception>
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (actionDescriptor is null)
            {
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            var filters = _filterProvider.GetFilters(configuration, actionDescriptor).ToList();

            foreach (var filterInfo in filters)
            {
                _rootLifetimeScope.InjectProperties(filterInfo.Instance);
            }

            // Use a fake scope to resolve the metadata for the filter.
            var rootLifetimeScope = configuration.DependencyResolver.GetRootLifetimeScope();
            if (rootLifetimeScope == null)
            {
                return filters;
            }

            using (var lifetimeScope = rootLifetimeScope.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag))
            {
                var filterContext = new FilterContext(
                    lifetimeScope,
                    actionDescriptor.ControllerDescriptor.ControllerType,
                    filters,
                    new Dictionary<AutofacFilterCategory, List<FilterPredicateMetadata>>
                    {
                        { AutofacFilterCategory.ActionFilter, new List<FilterPredicateMetadata>() },
                        { AutofacFilterCategory.ActionFilterOverride, new List<FilterPredicateMetadata>() },
                        { AutofacFilterCategory.AuthenticationFilter, new List<FilterPredicateMetadata>() },
                        { AutofacFilterCategory.AuthenticationFilterOverride, new List<FilterPredicateMetadata>() },
                        { AutofacFilterCategory.AuthorizationFilter, new List<FilterPredicateMetadata>() },
                        { AutofacFilterCategory.AuthorizationFilterOverride, new List<FilterPredicateMetadata>() },
                        { AutofacFilterCategory.ExceptionFilter, new List<FilterPredicateMetadata>() },
                        { AutofacFilterCategory.ExceptionFilterOverride, new List<FilterPredicateMetadata>() },
                    });

                // Controller scoped override filters (NOOP kind).
                ResolveScopedNoopFilterOverrides(filterContext, FilterScope.Controller, lifetimeScope, actionDescriptor);

                // Action scoped override filters (NOOP kind).
                ResolveScopedNoopFilterOverrides(filterContext, FilterScope.Action, lifetimeScope, actionDescriptor);

                // Controller scoped override filters.
                ResolveAllScopedFilterOverrides(filterContext, FilterScope.Controller, lifetimeScope, actionDescriptor);

                // Action scoped override filters.
                ResolveAllScopedFilterOverrides(filterContext, FilterScope.Action, lifetimeScope, actionDescriptor);

                // Controller scoped filters.
                ResolveAllScopedFilters(filterContext, FilterScope.Controller, lifetimeScope, actionDescriptor);

                // Action scoped filters.
                ResolveAllScopedFilters(filterContext, FilterScope.Action, lifetimeScope, actionDescriptor);
            }

            return filters;
        }

        private static void ResolveScopedNoopFilterOverrides(
            FilterContext filterContext,
            FilterScope scope,
            ILifetimeScope lifeTimeScope,
            HttpActionDescriptor descriptor)
        {
            ResolveScopedOverrideFilter(filterContext, scope, AutofacFilterCategory.ActionFilterOverride, lifeTimeScope, descriptor);
            ResolveScopedOverrideFilter(filterContext, scope, AutofacFilterCategory.AuthenticationFilterOverride, lifeTimeScope, descriptor);
            ResolveScopedOverrideFilter(filterContext, scope, AutofacFilterCategory.AuthorizationFilterOverride, lifeTimeScope, descriptor);
            ResolveScopedOverrideFilter(filterContext, scope, AutofacFilterCategory.ExceptionFilterOverride, lifeTimeScope, descriptor);
        }

        private static void ResolveAllScopedFilterOverrides(
            FilterContext filterContext,
            FilterScope scope,
            ILifetimeScope lifeTimeScope,
            HttpActionDescriptor descriptor)
        {
            ResolveScopedFilter<IAutofacContinuationActionFilter, ContinuationActionFilterOverrideWrapper>(
                filterContext, scope, lifeTimeScope, descriptor, hs => new ContinuationActionFilterOverrideWrapper(hs), AutofacFilterCategory.ActionFilterOverride);
            ResolveScopedFilter<IAutofacAuthenticationFilter, AuthenticationFilterOverrideWrapper>(
                filterContext, scope, lifeTimeScope, descriptor, hs => new AuthenticationFilterOverrideWrapper(hs), AutofacFilterCategory.AuthenticationFilterOverride);
            ResolveScopedFilter<IAutofacAuthorizationFilter, AuthorizationFilterOverrideWrapper>(
                filterContext, scope, lifeTimeScope, descriptor, hs => new AuthorizationFilterOverrideWrapper(hs), AutofacFilterCategory.AuthorizationFilterOverride);
            ResolveScopedFilter<IAutofacExceptionFilter, ExceptionFilterOverrideWrapper>(
                filterContext, scope, lifeTimeScope, descriptor, hs => new ExceptionFilterOverrideWrapper(hs), AutofacFilterCategory.ExceptionFilterOverride);
        }

        private static void ResolveAllScopedFilters(FilterContext filterContext, FilterScope scope, ILifetimeScope lifeTimeScope, HttpActionDescriptor descriptor)
        {
            ResolveScopedFilter<IAutofacContinuationActionFilter, ContinuationActionFilterWrapper>(
                filterContext, scope, lifeTimeScope, descriptor, hs => new ContinuationActionFilterWrapper(hs), AutofacFilterCategory.ActionFilter);
            ResolveScopedFilter<IAutofacAuthenticationFilter, AuthenticationFilterWrapper>(
                filterContext, scope, lifeTimeScope, descriptor, hs => new AuthenticationFilterWrapper(hs), AutofacFilterCategory.AuthenticationFilter);
            ResolveScopedFilter<IAutofacAuthorizationFilter, AuthorizationFilterWrapper>(
                filterContext, scope, lifeTimeScope, descriptor, hs => new AuthorizationFilterWrapper(hs), AutofacFilterCategory.AuthorizationFilter);
            ResolveScopedFilter<IAutofacExceptionFilter, ExceptionFilterWrapper>(
                filterContext, scope, lifeTimeScope, descriptor, hs => new ExceptionFilterWrapper(hs), AutofacFilterCategory.ExceptionFilter);
        }

        private static void ResolveScopedFilter<TFilter, TWrapper>(
            FilterContext filterContext,
            FilterScope scope,
            ILifetimeScope lifeTimeScope,
            HttpActionDescriptor descriptor,
            Func<HashSet<FilterMetadata>, TWrapper> wrapperFactory,
            AutofacFilterCategory filterCategory)
            where TFilter : class
            where TWrapper : class, IFilter
        {
            var filters = filterContext.LifetimeScope.Resolve<IEnumerable<Meta<Lazy<TFilter>>>>();

            // We'll store the unique filter registrations here until we create the wrapper.
            HashSet<FilterMetadata>? metadataSet = null;

            foreach (var filter in filters)
            {
                var metadata = filter.Metadata.TryGetValue(FilterMetadataKey, out var metadataAsObject)
                    ? metadataAsObject as FilterMetadata
                    : null;

                // Match the filter category (action filter, authentication, the overrides, etc).
                if (metadata != null)
                {
                    // Each individual predicate of the filter 'could' match the action descriptor.
                    // The HashSet makes sure the same filter doesn't go in twice.
                    foreach (var filterRegistration in metadata.PredicateSet)
                    {
                        if (FilterMatches(scope, filterCategory, lifeTimeScope, descriptor, filterRegistration))
                        {
                            if (metadataSet == null)
                            {
                                // Don't define a hash set if something has already been registered (should just be the IOverrideFilters).
                                if (!MatchingFilterAlreadyAdded(filterContext, filterCategory, lifeTimeScope, descriptor, filterRegistration))
                                {
                                    metadataSet = new HashSet<FilterMetadata>
                                    {
                                        metadata,
                                    };

                                    filterContext.AddedFilters[filterCategory].Add(filterRegistration);
                                }
                            }
                            else
                            {
                                metadataSet.Add(metadata);
                            }
                        }
                    }
                }
            }

            if (metadataSet != null)
            {
                // Declare our wrapper (telling it which filters it is responsible for)
                var wrapper = wrapperFactory(metadataSet);
                filterContext.Filters.Add(new FilterInfo(wrapper, scope));
            }
        }

        private static void ResolveScopedOverrideFilter(
            FilterContext filterContext,
            FilterScope scope,
            AutofacFilterCategory filterCategory,
            ILifetimeScope lifeTimeScope,
            HttpActionDescriptor descriptor)
        {
            var filters = filterContext.LifetimeScope.Resolve<IEnumerable<Meta<IOverrideFilter>>>();

            foreach (var filter in filters)
            {
                var metadata = filter.Metadata.TryGetValue(FilterMetadataKey, out var metadataAsObject)
                    ? metadataAsObject as FilterMetadata
                    : null;

                if (metadata != null)
                {
                    foreach (var filterRegistration in metadata.PredicateSet)
                    {
                        if (FilterMatchesAndNotAlreadyAdded(filterContext, scope, filterCategory, lifeTimeScope, filterRegistration, descriptor))
                        {
                            filterContext.Filters.Add(new FilterInfo(filter.Value, scope));
                            filterContext.AddedFilters[filterCategory].Add(filterRegistration);
                        }
                    }
                }
            }
        }

        private static bool MatchingFilterAlreadyAdded(FilterContext filterContext, AutofacFilterCategory filterCategory, ILifetimeScope lifeTimeScope, HttpActionDescriptor descriptor, FilterPredicateMetadata metadata)
        {
            var filters = filterContext.AddedFilters[filterCategory];
            return filters.Any(filter => filter.Scope == metadata.Scope &&
                                         (filter.Predicate == null || filter.Predicate(lifeTimeScope, descriptor)));
        }

        private static bool FilterMatches(
            FilterScope scope,
            AutofacFilterCategory filterCategory,
            ILifetimeScope lifeTimeScope,
            HttpActionDescriptor descriptor,
            FilterPredicateMetadata metadata)
        {
            return metadata.FilterCategory == filterCategory &&
                   metadata.Scope == scope &&
                   (metadata.Predicate == null || metadata.Predicate(lifeTimeScope, descriptor));
        }

        private static bool FilterMatchesAndNotAlreadyAdded(
            FilterContext filterContext,
            FilterScope scope,
            AutofacFilterCategory filterCategory,
            ILifetimeScope lifeTimeScope,
            FilterPredicateMetadata metadata,
            HttpActionDescriptor descriptor)
        {
            return FilterMatches(scope, filterCategory, lifeTimeScope, descriptor, metadata) &&
                   !MatchingFilterAlreadyAdded(filterContext, filterCategory, lifeTimeScope, descriptor, metadata);
        }
    }
}
