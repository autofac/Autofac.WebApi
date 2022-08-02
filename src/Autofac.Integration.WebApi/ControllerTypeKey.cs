// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.WebApi;

/// <summary>
/// A service key used to register services per controller type.
/// </summary>
internal class ControllerTypeKey : IEquatable<ControllerTypeKey>
{
    /// <summary>
    /// Gets the type of the controller.
    /// </summary>
    public Type ControllerType { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ControllerTypeKey"/> class.
    /// </summary>
    /// <param name="controllerType">Type of the controller.</param>
    public ControllerTypeKey(Type controllerType)
    {
        ControllerType = controllerType ?? throw new ArgumentNullException(nameof(controllerType));
    }

    /// <summary>
    /// Determines whether the specified <see cref="object" /> is equal to the current <see cref="object" />.
    /// </summary>
    /// <param name="other">The key to which the current key is being compared.</param>
    /// <returns>
    /// true if the specified <see cref="object" /> is equal to the current <see cref="object" />; otherwise, false.
    /// </returns>
    public bool Equals(ControllerTypeKey? other)
    {
        return other != null && other.ControllerType.IsAssignableFrom(ControllerType);
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current <see cref="object"/>.</param>
    /// <returns>
    /// true if the specified <see cref="object"/> is equal to the current <see cref="object"/>; otherwise, false.
    /// </returns>
    /// <exception cref="NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
    public override bool Equals(object obj)
    {
        return Equals(obj as ControllerTypeKey);
    }

    /// <summary>
    /// Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="object"/>.
    /// </returns>
    public override int GetHashCode()
    {
        return ControllerType.IsImport ? ControllerType.GUID.GetHashCode() : ControllerType.GetHashCode();
    }
}
