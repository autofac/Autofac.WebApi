// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Dependencies;
using Autofac.Core.Lifetime;

namespace Autofac.Integration.WebApi;

/// <summary>
/// Autofac implementation of the <see cref="IDependencyResolver"/> interface.
/// </summary>
public class AutofacWebApiDependencyResolver : IDependencyResolver
{
    private bool _disposed;
    private readonly IDependencyScope _rootDependencyScope;
    private readonly Action<ContainerBuilder>? _configurationAction;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutofacWebApiDependencyResolver"/> class.
    /// </summary>
    /// <param name="container">The container that nested lifetime scopes will be create from.</param>
    /// <param name="configurationAction">A configuration action that will execute during lifetime scope creation.</param>
    public AutofacWebApiDependencyResolver(ILifetimeScope container, Action<ContainerBuilder> configurationAction)
        : this(container)
    {
        _configurationAction = configurationAction ?? throw new ArgumentNullException(nameof(configurationAction));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutofacWebApiDependencyResolver"/> class.
    /// </summary>
    /// <param name="container">The container that nested lifetime scopes will be create from.</param>
    public AutofacWebApiDependencyResolver(ILifetimeScope container)
    {
        Container = container ?? throw new ArgumentNullException(nameof(container));
        _rootDependencyScope = new AutofacWebApiDependencyScope(container);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="AutofacWebApiDependencyResolver"/> class.
    /// </summary>
    ~AutofacWebApiDependencyResolver()
    {
        Dispose(false);
    }

    /// <summary>
    /// Gets the root container provided to the dependency resolver.
    /// </summary>
    public ILifetimeScope Container { get; }

    /// <summary>
    /// Try to get a service of the given type.
    /// </summary>
    /// <param name="serviceType">Type of service to request.</param>
    /// <returns>An instance of the service, or null if the service is not found.</returns>
    public virtual object GetService(Type serviceType)
    {
        return _rootDependencyScope.GetService(serviceType);
    }

    /// <summary>
    /// Try to get a list of services of the given type.
    /// </summary>
    /// <param name="serviceType">ControllerType of services to request.</param>
    /// <returns>An enumeration (possibly empty) of the service.</returns>
    public virtual IEnumerable<object> GetServices(Type serviceType)
    {
        return _rootDependencyScope.GetServices(serviceType);
    }

    /// <summary>
    /// Starts a resolution scope. Objects which are resolved in the given scope will belong to
    /// that scope, and when the scope is disposed, those objects are returned to the container.
    /// </summary>
    /// <returns>
    /// The dependency scope.
    /// </returns>
    public IDependencyScope BeginScope()
    {
        var lifetimeScope = _configurationAction == null
                                ? Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag)
                                : Container.BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag, _configurationAction);
        return new AutofacWebApiDependencyScope(lifetimeScope);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources;
    /// <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_rootDependencyScope != null)
                {
                    _rootDependencyScope.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
