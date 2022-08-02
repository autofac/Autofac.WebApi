// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;
using Autofac.Features.Metadata;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Configures the controller descriptor with per-controller services from the container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class AutofacControllerConfigurationAttribute : Attribute, IControllerConfiguration
    {
        private const string InitializedKey = "InjectControllerServicesAttributeInitialized";

        /// <summary>
        /// Metadata key that signifies existing controller services should be cleared.
        /// </summary>
        internal const string ClearServiceListKey = "ClearServiceList";

        /// <summary>
        /// Callback invoked to set per-controller overrides for this controllerDescriptor.
        /// </summary>
        /// <param name="controllerSettings">The controller settings to initialize.</param>
        /// <param name="controllerDescriptor">The controller descriptor. Note that the
        /// <see cref="System.Web.Http.Controllers.HttpControllerDescriptor"/> can be
        /// associated with the derived controller type given that <see cref="System.Web.Http.Controllers.IControllerConfiguration"/>
        /// is inherited.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="controllerSettings" /> or <paramref name="controllerDescriptor" /> is <see langword="null" />.
        /// </exception>
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerSettings == null)
            {
                throw new ArgumentNullException(nameof(controllerSettings));
            }

            if (controllerDescriptor == null)
            {
                throw new ArgumentNullException(nameof(controllerDescriptor));
            }

            if (controllerDescriptor.Configuration == null)
            {
                return;
            }

            if (!controllerDescriptor.Properties.TryAdd(InitializedKey, null))
            {
                return;
            }

            var container = controllerDescriptor.Configuration.DependencyResolver.GetRootLifetimeScope();
            if (container == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        AutofacControllerConfigurationAttributeResources.DependencyResolverMissing,
                        typeof(AutofacWebApiDependencyResolver).Name,
                        typeof(AutofacControllerConfigurationAttribute).Name));
            }

            var controllerServices = controllerSettings.Services;
            var serviceKey = new ControllerTypeKey(controllerDescriptor.ControllerType);

            UpdateControllerService<IHttpActionInvoker>(controllerServices, container, serviceKey);
            UpdateControllerService<IHttpActionSelector>(controllerServices, container, serviceKey);
            UpdateControllerService<IActionValueBinder>(controllerServices, container, serviceKey);
            UpdateControllerService<IBodyModelValidator>(controllerServices, container, serviceKey);
            UpdateControllerService<IContentNegotiator>(controllerServices, container, serviceKey);
            UpdateControllerService<IHttpControllerActivator>(controllerServices, container, serviceKey);
            UpdateControllerService<ModelMetadataProvider>(controllerServices, container, serviceKey);

            UpdateControllerServices<ModelBinderProvider>(controllerServices, container, serviceKey);
            UpdateControllerServices<ModelValidatorProvider>(controllerServices, container, serviceKey);
            UpdateControllerServices<ValueProviderFactory>(controllerServices, container, serviceKey);

            UpdateControllerFormatters(controllerSettings.Formatters, container, serviceKey);
        }

        private static void UpdateControllerService<T>(ServicesContainer services, IComponentContext container, ControllerTypeKey serviceKey)
            where T : class
        {
            var instance = container.ResolveOptionalKeyed<Meta<T>>(serviceKey);
            var baseControllerType = serviceKey.ControllerType.BaseType;
            while (instance == null && baseControllerType != typeof(ApiController))
            {
                var baseServiceKey = new ControllerTypeKey(baseControllerType);
                instance = container.ResolveOptionalKeyed<Meta<T>>(baseServiceKey);
                baseControllerType = baseServiceKey.ControllerType.BaseType;
            }

            if (instance != null)
            {
                services.Replace(typeof(T), instance.Value);
            }
        }

        private static void UpdateControllerServices<T>(ServicesContainer services, IComponentContext container, ControllerTypeKey serviceKey)
            where T : class
        {
            var resolvedInstances = container.ResolveOptionalKeyed<IEnumerable<Meta<T>>>(serviceKey).ToArray();

            if (resolvedInstances.Any(service => ClearExistingServices(service.Metadata)))
            {
                services.Clear(typeof(T));
            }

            foreach (var instance in resolvedInstances)
            {
                services.Add(typeof(T), instance.Value);
            }
        }

        private static void UpdateControllerFormatters(ICollection<MediaTypeFormatter> collection, IComponentContext container, ControllerTypeKey serviceKey)
        {
            var formatters = container.ResolveOptionalKeyed<IEnumerable<Meta<MediaTypeFormatter>>>(serviceKey).ToArray();

            if (formatters.Any(service => ClearExistingServices(service.Metadata)))
            {
                collection.Clear();
            }

            foreach (var formatter in formatters)
            {
                collection.Add(formatter.Value);
            }
        }

        private static bool ClearExistingServices(IDictionary<string, object?> metadata)
        {
            return metadata.TryGetValue(ClearServiceListKey, out var value) && value != null && (bool)value;
        }
    }
}
