// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Dependencies;

namespace Autofac.Integration.WebApi;

/// <summary>
/// Extension methods to the <see cref="IDependencyResolver"/> interface.
/// </summary>
public static class DependencyResolverExtensions
{
    /// <summary>
    /// Gets the root lifetime scope from the Autofac dependency resolver.
    /// </summary>
    /// <param name="dependencyResolver">
    /// The dependency resolver from which the root lifetime scope should be retrieved.
    /// </param>
    public static ILifetimeScope? GetRootLifetimeScope(this IDependencyResolver dependencyResolver)
    {
        var resolver = dependencyResolver as AutofacWebApiDependencyResolver;
        return resolver?.Container;
    }

    /// <summary>
    /// Gets the request lifetime scope from the Autofac dependency scope.
    /// </summary>
    /// <param name="dependencyScope">
    /// The dependency scope from which the request lifetime scope should be retrieved.
    /// </param>
    public static ILifetimeScope? GetRequestLifetimeScope(this IDependencyScope dependencyScope)
    {
        var scope = dependencyScope as AutofacWebApiDependencyScope;
        return scope?.LifetimeScope;
    }
}
