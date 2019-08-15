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
//
// Portions of this file come from the Microsoft ASP.NET web stack, licensed
// under the Apache 2.0 license. You may obtain a copy of the license at
// http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// This is an adapter responsible for wrapping the old style <see cref="IAutofacActionFilter"/>
    /// and converting it to a <see cref="IAutofacContinuationActionFilter"/>.
    /// </summary>
    /// <remarks>
    /// The adapter from old -> new is registered in <see cref="RegistrationExtensions.RegisterWebApiFilterProvider"/>.
    /// </remarks>
    internal class AutofacActionFilterAdapter : IAutofacContinuationActionFilter
    {
        private readonly IAutofacActionFilter _legacyFilter;

        public AutofacActionFilterAdapter(IAutofacActionFilter legacyFilter)
        {
            _legacyFilter = legacyFilter;
        }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            await _legacyFilter.OnActionExecutingAsync(actionContext, cancellationToken);

            if (actionContext.Response != null)
            {
                return actionContext.Response;
            }

            return await CallOnActionExecutedAsync(actionContext, cancellationToken, continuation);
        }

        /// <summary>
        /// The content of this method is taken from the ActionFilterAttribute code in the ASP.NET source, since
        /// that is basically the reference implementation for invoking an async filter's OnActionExecuted correctly.
        /// </summary>
        [SuppressMessage("Microsoft.CodeQuality", "CA1068", Justification = "Matching parameter order in original implementtion.")]
        private async Task<HttpResponseMessage> CallOnActionExecutedAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = null;
            ExceptionDispatchInfo exceptionInfo = null;

            try
            {
                response = await continuation();
            }
            catch (Exception e)
            {
                exceptionInfo = ExceptionDispatchInfo.Capture(e);
            }

            Exception exception;

            if (exceptionInfo == null)
            {
                exception = null;
            }
            else
            {
                exception = exceptionInfo.SourceException;
            }

            HttpActionExecutedContext executedContext = new HttpActionExecutedContext(actionContext, exception)
            {
                Response = response
            };

            try
            {
                await _legacyFilter.OnActionExecutedAsync(executedContext, cancellationToken);
            }
            catch
            {
                // Catch is running because OnActionExecuted threw an exception, so we just want to re-throw.
                // We also need to reset the response to forget about it since a filter threw an exception.
                actionContext.Response = null;
                throw;
            }

            if (executedContext.Response != null)
            {
                return executedContext.Response;
            }

            Exception newException = executedContext.Exception;

            if (newException != null)
            {
                if (newException == exception)
                {
                    exceptionInfo.Throw();
                }
                else
                {
                    throw newException;
                }
            }

            throw new InvalidOperationException();
        }
    }
}
