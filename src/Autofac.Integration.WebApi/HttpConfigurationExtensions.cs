// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// Extension methods for <see cref="HttpConfiguration"/> to enable Web API and Autofac integration.
    /// </summary>
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Makes the current <see cref="HttpRequestMessage"/> resolvable through the dependency scope.
        /// </summary>
        /// <param name="config">The HTTP server configuration.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="config" /> is <see langword="null" />.
        /// </exception>
        [Obsolete("The HttpRequestMessage must be registered using the RegisterHttpRequestMessage extension method on ContainerBuilder.", true)]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", Justification = "Method marked as obsolete to point to correct method.")]
        public static void RegisterHttpRequestMessage(this HttpConfiguration config)
        {
        }

        internal static void RegisterHttpRequestMessage(this HttpConfiguration config, ContainerBuilder builder)
        {
            if (config.MessageHandlers.OfType<CurrentRequestHandler>().Any()) return;

            builder.Register(c => HttpRequestMessageProvider.Current)
                .InstancePerRequest()
                .ExternallyOwned();

            config.MessageHandlers.Add(new CurrentRequestHandler());
        }
    }
}
