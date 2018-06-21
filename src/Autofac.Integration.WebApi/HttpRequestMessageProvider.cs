// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;

namespace Autofac.Integration.WebApi
{
    internal static class HttpRequestMessageProvider
    {
        private static readonly string Key = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture).Substring(0, 12);

        internal static HttpRequestMessage Current
        {
            get
            {
                var wrapper = (HttpRequestMessageWrapper)CallContext.LogicalGetData(Key);
                return wrapper?.Message;
            }

            set
            {
                var wrapper = value == null ? null : new HttpRequestMessageWrapper(value);
                CallContext.LogicalSetData(Key, wrapper);
            }
        }

        [Serializable]
        private sealed class HttpRequestMessageWrapper : MarshalByRefObject
        {
            [NonSerialized]
            [SuppressMessage("SA1401", "SA1401", Justification = "Field is only used during testing.")]
            internal readonly HttpRequestMessage Message;

            internal HttpRequestMessageWrapper(HttpRequestMessage message)
            {
                Message = message;
            }
        }
    }
}
