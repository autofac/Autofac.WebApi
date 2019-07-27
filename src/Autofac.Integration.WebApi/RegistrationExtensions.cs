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

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Adds registration syntax to the <see cref="ContainerBuilder"/> type.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Register types in the provided assemblies that implement <see cref="IHttpController"/> and
        /// match the default type name suffix of "Controller".
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="controllerAssemblies">Assemblies to scan for controllers.</param>
        /// <returns>Registration builder allowing the controller components to be customised.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterApiControllers(this ContainerBuilder builder, params Assembly[] controllerAssemblies)
        {
            return RegisterApiControllers(builder, "Controller", controllerAssemblies);
        }

        /// <summary>
        /// Register types in the provided assemblies that implement <see cref="IHttpController"/> and
        /// match the provided type name suffix.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="controllerSuffix">The type name suffix of the controllers.</param>
        /// <param name="controllerAssemblies">Assemblies to scan for controllers.</param>
        /// <returns>Registration builder allowing the controller components to be customised.</returns>
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterApiControllers(this ContainerBuilder builder, string controllerSuffix, params Assembly[] controllerAssemblies)
        {
            return builder.RegisterAssemblyTypes(controllerAssemblies)
                .Where(t => typeof(IHttpController).IsAssignableFrom(t) && t.Name.EndsWith(controllerSuffix, StringComparison.Ordinal));
        }

        /// <summary>
        /// Share one instance of the component within the context of a
        /// single <see cref="ApiController"/> request.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="lifetimeScopeTags">Additional tags applied for matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> is <see langword="null" />.
        /// </exception>
        [Obsolete("Instead of using the Web-API-specific InstancePerApiRequest, please switch to the InstancePerRequest shared registration extension from Autofac core.")]
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerApiRequest<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, params object[] lifetimeScopeTags)
        {
            return registration.InstancePerRequest(lifetimeScopeTags);
        }

        /// <summary>
        /// Share one instance of the component within the context of a controller type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="controllerType">The controller type.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerApiControllerType<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, Type controllerType)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            return InstancePerApiControllerType(registration, controllerType, false);
        }

        /// <summary>
        /// Share one instance of the component within the context of a controller type.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="controllerType">The controller type.</param>
        /// <param name="clearExistingServices">Clear the existing list of controller level services before adding.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerApiControllerType<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, Type controllerType, bool clearExistingServices)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var services = registration.RegistrationData.Services.ToArray();
            registration.RegistrationData.ClearServices();
            var defaultService = new TypedService(typeof(TLimit));
            registration.RegistrationData.AddServices(services.Where(s => s != defaultService));

            return registration.Keyed<TLimit>(new ControllerTypeKey(controllerType))
                .WithMetadata(AutofacControllerConfigurationAttribute.ClearServiceListKey, clearExistingServices);
        }

        /// <summary>
        /// Makes the current <see cref="HttpRequestMessage"/> resolvable through the dependency scope.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="config">The HTTP server configuration.</param>
        public static void RegisterHttpRequestMessage(this ContainerBuilder builder, HttpConfiguration config)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (config == null) throw new ArgumentNullException(nameof(config));

            config.RegisterHttpRequestMessage(builder);
        }

        /// <summary>
        /// Registers the <see cref="AutofacWebApiModelBinderProvider"/>.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void RegisterWebApiModelBinderProvider(this ContainerBuilder builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder.RegisterType<AutofacWebApiModelBinderProvider>()
                .As<ModelBinderProvider>()
                .SingleInstance();
        }

        /// <summary>
        /// Register types that implement <see cref="IModelBinder"/> in the provided assemblies.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="modelBinderAssemblies">Assemblies to scan for model binders.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> or <paramref name="modelBinderAssemblies" /> is <see langword="null" />.
        /// </exception>
        [Obsolete("Use the AsModelBinderForTypes() registration extension to register model binders and be sure to RegisterWebApiModelBinderProvider() in your container if you do. This method doesn't connect the model binders to the Autofac binder provider. It will be removed in a future version.")]
        public static IRegistrationBuilder<object, ScanningActivatorData, DynamicRegistrationStyle>
            RegisterWebApiModelBinders(this ContainerBuilder builder, params Assembly[] modelBinderAssemblies)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            if (modelBinderAssemblies == null) throw new ArgumentNullException(nameof(modelBinderAssemblies));

            return builder.RegisterAssemblyTypes(modelBinderAssemblies)
                .Where(type => type.IsAssignableTo<IModelBinder>())
                .As<IModelBinder>()
                .AsSelf()
                .SingleInstance();
        }

        /// <summary>
        /// Sets a provided registration to act as an <see cref="IModelBinder"/> for the specified list of types.
        /// </summary>
        /// <param name="registration">The registration for the type or object instance that will act as the model binder.</param>
        /// <param name="types">The list of model <see cref="Type"/> for which the <paramref name="registration" /> should be a model binder.</param>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
        /// <returns>An Autofac registration that can be modified as needed.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="registration" /> or <paramref name="types" /> is <see langword="null" />.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="types" /> is empty or contains all <see langword="null" /> values.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>
            AsModelBinderForTypes<TLimit, TActivatorData, TRegistrationStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> registration, params Type[] types)
            where TActivatorData : IConcreteActivatorData
            where TRegistrationStyle : SingleRegistrationStyle
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (types == null) throw new ArgumentNullException(nameof(types));

            var typeList = types.Where(type => type != null).ToList();
            if (typeList.Count == 0)
                throw new ArgumentException(RegistrationExtensionsResources.ListMustNotBeEmptyOrContainNulls, nameof(types));

            return registration.As<IModelBinder>().WithMetadata(AutofacWebApiModelBinderProvider.MetadataKey, typeList);
        }

        /// <summary>
        /// Registers the <see cref="AutofacWebApiFilterProvider"/>.
        /// </summary>
        /// <param name="configuration">Configuration of HttpServer instances.</param>
        /// <param name="builder">The container builder.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="builder" /> or <paramref name="configuration" /> is <see langword="null" />.
        /// </exception>
        public static void RegisterWebApiFilterProvider(this ContainerBuilder builder, HttpConfiguration configuration)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            configuration.Services.RemoveAll(typeof(IFilterProvider), provider => provider is ActionDescriptorFilterProvider);

            builder.Register(c => new AutofacWebApiFilterProvider(c.Resolve<ILifetimeScope>()))
                .As<IFilterProvider>()
                .SingleInstance(); // It would be nice to scope this per request.
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiActionFilterFor<TController>(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector)
                    where TController : IHttpController
        {
            return AsFilterFor<IAutofacActionFilter, TController>(registration, AutofacFilterCategory.ActionFilter, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiActionFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacActionFilter, TController>(registration, AutofacFilterCategory.ActionFilter);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> for all controllers.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiActionFilterForAllControllers(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            return AsFilterFor<IAutofacActionFilter>(registration, AutofacFilterCategory.ActionFilter, descriptor => true, FilterScope.Controller);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/>, based on a predicate that filters which actions it is applied to.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="predicate">A predicate that should return true if this filter should be applied to the specified action.</param>
        /// <param name="scope">The scope to apply the filter at (only Controller and Action supported).</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiActionFilterWhere(
                this IRegistrationBuilder<object, IConcreteActivatorData,
                SingleRegistrationStyle> registration, Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope = FilterScope.Action)
        {
            return AsFilterFor<IAutofacActionFilter>(registration, AutofacFilterCategory.ActionFilter, predicate, scope);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> override for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiActionFilterOverrideFor<TController>(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector)
                    where TController : IHttpController
        {
            return AsFilterFor<IAutofacActionFilter, TController>(registration, AutofacFilterCategory.ActionFilterOverride, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> override for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiActionFilterOverrideFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacActionFilter, TController>(registration, AutofacFilterCategory.ActionFilterOverride);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> override for all controllers.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiActionFilterOverrideForAllControllers(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            return AsFilterFor<IAutofacActionFilter>(registration, AutofacFilterCategory.ActionFilterOverride, descriptor => true, FilterScope.Controller);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacActionFilter"/> override, based on a predicate that filters which actions it is applied to.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="predicate">A predicate that should return true if this filter should be applied to the specified action.</param>
        /// <param name="scope">The scope to apply the filter at (only Controller and Action supported).</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiActionFilterOverrideWhere(
                this IRegistrationBuilder<object, IConcreteActivatorData,
                    SingleRegistrationStyle> registration, Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope = FilterScope.Action)
        {
            return AsFilterFor<IAutofacActionFilter>(registration, AutofacFilterCategory.ActionFilterOverride, predicate, scope);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthorizationFilterFor<TController>(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector)
            where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthorizationFilter, TController>(registration, AutofacFilterCategory.AuthorizationFilter, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthorizationFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthorizationFilter, TController>(registration, AutofacFilterCategory.AuthorizationFilter);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> for all controllers.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthorizationFilterForAllControllers(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            return AsFilterFor<IAutofacAuthorizationFilter>(registration, AutofacFilterCategory.AuthorizationFilter, descriptor => true, FilterScope.Controller);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/>, based on a predicate that filters which actions it is applied to.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="predicate">A predicate that should return true if this filter should be applied to the specified action.</param>
        /// <param name="scope">The scope to apply the filter at (only Controller and Action supported).</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthorizationFilterWhere(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope = FilterScope.Action)
        {
            return AsFilterFor<IAutofacAuthorizationFilter>(registration, AutofacFilterCategory.AuthorizationFilter, predicate, scope);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> override for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthorizationFilterOverrideFor<TController>(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Expression<Action<TController>> actionSelector)
            where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthorizationFilter, TController>(registration, AutofacFilterCategory.AuthorizationFilterOverride, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> override for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthorizationFilterOverrideFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthorizationFilter, TController>(registration, AutofacFilterCategory.AuthorizationFilterOverride);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> override for all controllers.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthorizationFilterOverrideForAllControllers(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            return AsFilterFor<IAutofacAuthorizationFilter>(registration, AutofacFilterCategory.AuthorizationFilterOverride, descriptor => true, FilterScope.Controller);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthorizationFilter"/> override, based on a predicate that filters which actions it is applied to.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="predicate">A predicate that should return true if this filter should be applied to the specified action.</param>
        /// <param name="scope">The scope to apply the filter at (only Controller and Action supported).</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthorizationFilterOverrideWhere(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope = FilterScope.Action)
        {
            return AsFilterFor<IAutofacAuthorizationFilter>(registration, AutofacFilterCategory.AuthorizationFilterOverride, predicate, scope);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiExceptionFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacExceptionFilter, TController>(registration, AutofacFilterCategory.ExceptionFilter, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiExceptionFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacExceptionFilter, TController>(registration, AutofacFilterCategory.ExceptionFilter);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> for all controllers.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiExceptionFilterForAllControllers(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            return AsFilterFor<IAutofacExceptionFilter>(registration, AutofacFilterCategory.ExceptionFilter, descriptor => true, FilterScope.Controller);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/>, based on a predicate that filters which actions it is applied to.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="predicate">A predicate that should return true if this filter should be applied to the specified action.</param>
        /// <param name="scope">The scope to apply the filter at (only Controller and Action supported).</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiExceptionFilterWhere(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope = FilterScope.Action)
        {
            return AsFilterFor<IAutofacExceptionFilter>(registration, AutofacFilterCategory.ExceptionFilter, predicate, scope);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> override for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiExceptionFilterOverrideFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacExceptionFilter, TController>(registration, AutofacFilterCategory.ExceptionFilterOverride, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> override for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiExceptionFilterOverrideFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacExceptionFilter, TController>(registration, AutofacFilterCategory.ExceptionFilterOverride);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> override for all controllers.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiExceptionFilterOverrideForAllControllers(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            return AsFilterFor<IAutofacExceptionFilter>(registration, AutofacFilterCategory.ExceptionFilterOverride, descriptor => true, FilterScope.Controller);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacExceptionFilter"/> override, based on a predicate that filters which actions it is applied to.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="predicate">A predicate that should return true if this filter should be applied to the specified action.</param>
        /// <param name="scope">The scope to apply the filter at (only Controller and Action supported).</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiExceptionFilterOverrideWhere(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope = FilterScope.Action)
        {
            return AsFilterFor<IAutofacExceptionFilter>(registration, AutofacFilterCategory.ExceptionFilterOverride, predicate, scope);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthenticationFilter"/> for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthenticationFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthenticationFilter, TController>(registration, AutofacFilterCategory.AuthenticationFilter, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthenticationFilter"/> for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthenticationFilterFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthenticationFilter, TController>(registration, AutofacFilterCategory.AuthenticationFilter);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAuthenticationFilter"/> for all controllers.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthenticationFilterForAllControllers(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            return AsFilterFor<IAutofacAuthenticationFilter>(registration, AutofacFilterCategory.AuthenticationFilter, descriptor => true, FilterScope.Controller);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAuthenticationFilter"/>, based on a predicate that filters which actions it is applied to.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="predicate">A predicate that should return true if this filter should be applied to the specified action.</param>
        /// <param name="scope">The scope to apply the filter at (only Controller and Action supported).</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthenticationFilterWhere(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope = FilterScope.Action)
        {
            return AsFilterFor<IAutofacAuthenticationFilter>(registration, AutofacFilterCategory.AuthenticationFilter, predicate, scope);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthenticationFilter"/> override for the specified controller action.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <param name="actionSelector">The action selector.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthenticationFilterOverrideFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthenticationFilter, TController>(registration, AutofacFilterCategory.AuthenticationFilterOverride, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAutofacAuthenticationFilter"/> override for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthenticationFilterOverrideFor<TController>(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
                where TController : IHttpController
        {
            return AsFilterFor<IAutofacAuthenticationFilter, TController>(registration, AutofacFilterCategory.AuthenticationFilterOverride);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAuthenticationFilter"/> override for all controllers.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthenticationFilterOverrideForAllControllers(this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration)
        {
            return AsFilterFor<IAuthenticationFilter>(registration, AutofacFilterCategory.AuthenticationFilterOverride, descriptor => true, FilterScope.Controller);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IAuthenticationFilter"/> override, based on a predicate that filters which actions it is applied to.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="predicate">A predicate that should return true if this filter should be applied to the specified action.</param>
        /// <param name="scope">The scope to apply the filter at (only Controller and Action supported).</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        public static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsWebApiAuthenticationFilterOverrideWhere(
                this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope = FilterScope.Action)
        {
            return AsFilterFor<IAuthenticationFilter>(registration, AutofacFilterCategory.AuthenticationFilterOverride, predicate, scope);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IOverrideFilter"/> for the specified controller action.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="actionSelector">The action selector.</param>
        public static void OverrideWebApiActionFilterFor<TController>(this ContainerBuilder builder, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            AsOverrideFor<IActionFilter, TController>(builder, AutofacFilterCategory.ActionFilterOverride, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IOverrideFilter"/> for the specified controller.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void OverrideWebApiActionFilterFor<TController>(this ContainerBuilder builder)
                where TController : IHttpController
        {
            AsOverrideFor<IActionFilter, TController>(builder, AutofacFilterCategory.ActionFilterOverride);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IOverrideFilter"/> for the specified controller action.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="actionSelector">The action selector.</param>
        public static void OverrideWebApiAuthorizationFilterFor<TController>(this ContainerBuilder builder, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            AsOverrideFor<IAuthorizationFilter, TController>(builder, AutofacFilterCategory.AuthorizationFilterOverride, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IOverrideFilter"/> for the specified controller.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void OverrideWebApiAuthorizationFilterFor<TController>(this ContainerBuilder builder)
                where TController : IHttpController
        {
            AsOverrideFor<IAuthorizationFilter, TController>(builder, AutofacFilterCategory.AuthorizationFilterOverride);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IOverrideFilter"/> for the specified controller action.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="actionSelector">The action selector.</param>
        public static void OverrideWebApiExceptionFilterFor<TController>(this ContainerBuilder builder, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            AsOverrideFor<IExceptionFilter, TController>(builder, AutofacFilterCategory.ExceptionFilterOverride, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IOverrideFilter"/> for the specified controller.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void OverrideWebApiExceptionFilterFor<TController>(this ContainerBuilder builder)
                where TController : IHttpController
        {
            AsOverrideFor<IExceptionFilter, TController>(builder, AutofacFilterCategory.ExceptionFilterOverride);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IOverrideFilter"/> for the specified controller action.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <param name="actionSelector">The action selector.</param>
        public static void OverrideWebApiAuthenticationFilterFor<TController>(this ContainerBuilder builder, Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            AsOverrideFor<IAuthenticationFilter, TController>(builder, AutofacFilterCategory.AuthenticationFilterOverride, actionSelector);
        }

        /// <summary>
        /// Sets the provided registration to act as an <see cref="IOverrideFilter"/> for the specified controller.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        public static void OverrideWebApiAuthenticationFilterFor<TController>(this ContainerBuilder builder)
                where TController : IHttpController
        {
            AsOverrideFor<IAuthenticationFilter, TController>(builder, AutofacFilterCategory.AuthenticationFilterOverride);
        }

        private static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsFilterFor<TFilter>(
                IRegistrationBuilder<object, IConcreteActivatorData,
                SingleRegistrationStyle> registration,
                AutofacFilterCategory filterCategory,
                Func<HttpActionDescriptor, bool> predicate,
                FilterScope scope)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (scope != FilterScope.Action && scope != FilterScope.Controller) throw new InvalidEnumArgumentException(nameof(scope), (int)scope, typeof(FilterScope));

            var limitType = registration.ActivatorData.Activator.LimitType;

            if (!limitType.IsAssignableTo<TFilter>())
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    RegistrationExtensionsResources.MustBeAssignableToFilterType,
                    limitType.FullName,
                    typeof(TFilter).FullName);
                throw new ArgumentException(message, nameof(registration));
            }

            // Get the filter metadata set.
            registration = registration.GetOrCreateMetadata(out FilterMetadata filterMeta);

            var registrationMetadata = new FilterPredicateMetadata
            {
                Scope = scope,
                FilterCategory = filterCategory,
                Predicate = predicate
            };

            filterMeta.PredicateSet.Add(registrationMetadata);

            return registration.As<TFilter>();
        }

        private static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsFilterFor<TFilter, TController>(IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration, AutofacFilterCategory filterCategory)
                where TController : IHttpController
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));

            var limitType = registration.ActivatorData.Activator.LimitType;

            if (!limitType.IsAssignableTo<TFilter>())
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    RegistrationExtensionsResources.MustBeAssignableToFilterType,
                    limitType.FullName,
                    typeof(TFilter).FullName);
                throw new ArgumentException(message, nameof(registration));
            }

            // Get the filter metadata set.
            registration = registration.GetOrCreateMetadata(out FilterMetadata filterMeta);

            var registrationMetadata = new FilterPredicateMetadata
            {
                Scope = FilterScope.Controller,
                FilterCategory = filterCategory,
                Predicate = descriptor => descriptor.ControllerDescriptor.ControllerType == typeof(TController)
            };

            filterMeta.PredicateSet.Add(registrationMetadata);

            return registration.As<TFilter>();
        }

        private static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle>
            AsFilterFor<TFilter, TController>(
                IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
                AutofacFilterCategory filterCategory,
                Expression<Action<TController>> actionSelector)
                where TController : IHttpController
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            if (actionSelector == null) throw new ArgumentNullException(nameof(actionSelector));

            var limitType = registration.ActivatorData.Activator.LimitType;

            if (!limitType.IsAssignableTo<TFilter>())
            {
                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    RegistrationExtensionsResources.MustBeAssignableToFilterType,
                    limitType.FullName,
                    typeof(TFilter).FullName);
                throw new ArgumentException(message, nameof(registration));
            }

            // Get the filter metadata set.
            registration = registration.GetOrCreateMetadata(out FilterMetadata filterMeta);

            var method = GetMethodInfo(actionSelector);

            var registrationMetadata = new FilterPredicateMetadata
            {
                Scope = FilterScope.Action,
                FilterCategory = filterCategory,
                Predicate = descriptor => typeof(TController).IsAssignableFrom(descriptor.ControllerDescriptor.ControllerType) &&
                                          ActionMethodMatches(descriptor, method)
            };

            filterMeta.PredicateSet.Add(registrationMetadata);

            return registration.As<TFilter>();
        }

        private static void AsOverrideFor<TFilter, TController>(ContainerBuilder builder, AutofacFilterCategory filterCategory)
        {
            builder.RegisterInstance(new AutofacOverrideFilter(typeof(TFilter)))
                  .As<IOverrideFilter>()
                  .GetOrCreateOverrideMetadata(out var filterMetadata);

            filterMetadata.PredicateSet.Add(new FilterPredicateMetadata
            {
                Scope = FilterScope.Controller,
                FilterCategory = filterCategory,
                Predicate = descriptor => typeof(TController).IsAssignableFrom(descriptor.ControllerDescriptor.ControllerType)
            });
        }

        private static void AsOverrideFor<TFilter, TController>(ContainerBuilder builder, AutofacFilterCategory filterCategory, Expression<Action<TController>> actionSelector)
        {
            if (actionSelector == null) throw new ArgumentNullException(nameof(actionSelector));

            var methodInfo = GetMethodInfo(actionSelector);

            builder.RegisterInstance(new AutofacOverrideFilter(typeof(TFilter)))
                .As<IOverrideFilter>()
                .GetOrCreateOverrideMetadata(out var filterMetadata);

            filterMetadata.PredicateSet.Add(new FilterPredicateMetadata
            {
                Scope = FilterScope.Action,
                FilterCategory = filterCategory,
                Predicate = descriptor => typeof(TController).IsAssignableFrom(descriptor.ControllerDescriptor.ControllerType) &&
                                          ActionMethodMatches(descriptor, methodInfo)
            });
        }

        private static MethodInfo GetMethodInfo(LambdaExpression expression)
        {
            var outermostExpression = expression.Body as MethodCallExpression;

            if (outermostExpression == null)
                throw new ArgumentException(RegistrationExtensionsResources.InvalidActionExpress);

            return outermostExpression.Method;
        }

        /// <summary>
        /// Retrieve or create filter metadata. We want to maintain the fluent flow when we change
        /// registration metadata so we'll do that here.
        /// </summary>
        private static IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> GetOrCreateMetadata (
            this IRegistrationBuilder<object, IConcreteActivatorData, SingleRegistrationStyle> registration,
            out FilterMetadata filterMeta)
        {
            if (registration.RegistrationData.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var filterDataObj))
            {
                filterMeta = (FilterMetadata)filterDataObj;
            }
            else
            {
                filterMeta = new FilterMetadata();
                registration = registration.WithMetadata(AutofacWebApiFilterProvider.FilterMetadataKey, filterMeta);
            }

            return registration;
        }

        /// <summary>
        /// Retrieve or create filter metadata for override filters. We want to maintain the fluent flow when we change
        /// registration metadata so we'll do that here.
        /// </summary>
        private static IRegistrationBuilder<AutofacOverrideFilter, SimpleActivatorData, SingleRegistrationStyle> GetOrCreateOverrideMetadata(
            this IRegistrationBuilder<AutofacOverrideFilter, SimpleActivatorData, SingleRegistrationStyle> registration,
            out FilterMetadata filterMeta)
        {
            if (registration.RegistrationData.Metadata.TryGetValue(AutofacWebApiFilterProvider.FilterMetadataKey, out var filterDataObj))
            {
                filterMeta = (FilterMetadata)filterDataObj;
            }
            else
            {
                filterMeta = new FilterMetadata();
                registration = registration.WithMetadata(AutofacWebApiFilterProvider.FilterMetadataKey, filterMeta);
            }

            return registration;
        }

        private static bool ActionMethodMatches(HttpActionDescriptor action, MethodInfo knownMethod)
        {
            var reflectedDescriptor = action as ReflectedHttpActionDescriptor;

            if (reflectedDescriptor == null)
            {
                return false;
            }

            // Including fix for Issue #10 in new registration style:
            // Comparing MethodInfo.MethodHandle rather than just MethodInfo equality
            // because MethodInfo equality fails on a derived controller if the base class method
            // isn't marked virtual... but MethodHandle correctly compares regardless.
            return reflectedDescriptor.MethodInfo.GetBaseDefinition().MethodHandle == knownMethod.GetBaseDefinition().MethodHandle;
        }
    }
}