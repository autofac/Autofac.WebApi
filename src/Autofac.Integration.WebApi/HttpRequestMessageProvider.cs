// This software is part of the Autofac IoC container
// Copyright (c) 2013 Autofac Contributors
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

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;

namespace Autofac.Integration.WebApi
{
    internal static class HttpRequestMessageProvider
    {
        private static readonly AsyncLocal<HttpRequestMessageHolder> CurrentRequest = new AsyncLocal<HttpRequestMessageHolder>();

        internal static HttpRequestMessage Current
        {
            get
            {
                return CurrentRequest.Value?.Message;
            }

            set
            {
                var holder = CurrentRequest.Value;
                if (holder != null)
                {
                    // Clear current HttpContext trapped in the AsyncLocals, as its done.
                    holder.Message = null;
                }

                if (value != null)
                {
                    // Use an object indirection to hold the HttpContext in the AsyncLocal,
                    // so it can be cleared in all ExecutionContexts when its cleared.
                    CurrentRequest.Value = new HttpRequestMessageHolder { Message = value };
                }
            }
        }

        private sealed class HttpRequestMessageHolder
        {
            [SuppressMessage("SA1401", "SA1401", Justification = "Field is only used during testing.")]
            public HttpRequestMessage Message;
        }
    }
}
