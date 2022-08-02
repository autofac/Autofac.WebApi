// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http;
using System.Web.Http.ModelBinding;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi;

/// <summary>
/// Autofac implementation of the <see cref="ModelBinderProvider"/> class.
/// </summary>
public class AutofacWebApiModelBinderProvider : ModelBinderProvider
{
    /// <summary>
    /// Metadata key for the supported model types.
    /// </summary>
    internal const string MetadataKey = "SupportedModelTypes";

    /// <summary>
    /// Find a binder for the given type.
    /// </summary>
    /// <param name="configuration">A configuration object.</param>
    /// <param name="modelType">The type of the model to bind against.</param>
    /// <returns>A binder, which can attempt to bind this type. Or null if the binder knows statically that it will never be able to bind the type.</returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="configuration" /> is <see langword="null" />.
    /// </exception>
    public override IModelBinder? GetBinder(HttpConfiguration configuration, Type modelType)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var modelBinders = configuration.DependencyResolver
            .GetServices(typeof(Meta<Lazy<IModelBinder>>))
            .Cast<Meta<Lazy<IModelBinder>>>();

        foreach (var binder in modelBinders)
        {
            if (binder.Metadata.TryGetValue(MetadataKey, out var metadataAsObject) && metadataAsObject != null)
            {
                if (((List<Type>)metadataAsObject).Contains(modelType))
                {
                    return binder.Value.Value;
                }
            }
        }

        return null;
    }
}
