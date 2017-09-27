﻿// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
// http://autofac.org
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
        internal static bool IsHttpRequestMessageTrackingEnabled = false;

        /// <summary>
        /// Makes the current <see cref="HttpRequestMessage"/> resolvable through the dependency scope.
        /// </summary>
        /// <param name="config">The HTTP server configuration.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="config" /> is <see langword="null" />.
        /// </exception>
        public static void RegisterHttpRequestMessage(this HttpConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            if (!config.MessageHandlers.OfType<CurrentRequestHandler>().Any())
            {
                config.MessageHandlers.Add(new CurrentRequestHandler());
            }

            IsHttpRequestMessageTrackingEnabled = true;
        }
    }
}
