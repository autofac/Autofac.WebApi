// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacActionFilterAdapter"/> class.
        /// </summary>
        /// <param name="legacyFilter">
        /// Original style <see cref="IAutofacActionFilter"/> to wrap as a continuation filter.
        /// </param>
        public AutofacActionFilterAdapter(IAutofacActionFilter legacyFilter)
        {
            _legacyFilter = legacyFilter;
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            await _legacyFilter.OnActionExecutingAsync(actionContext, cancellationToken).ConfigureAwait(false);

            if (actionContext.Response != null)
            {
                return actionContext.Response;
            }

            return await CallOnActionExecutedAsync(actionContext, cancellationToken, continuation).ConfigureAwait(false);
        }

        /// <summary>
        /// The content of this method is taken from the ActionFilterAttribute code in the ASP.NET source, since
        /// that is basically the reference implementation for invoking an async filter's OnActionExecuted correctly.
        /// </summary>
        [SuppressMessage("Microsoft.CodeQuality", "CA1068", Justification = "Matching parameter order in original implementation.")]
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to capture any exception that occurs.")]
        private async Task<HttpResponseMessage> CallOnActionExecutedAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            cancellationToken.ThrowIfCancellationRequested();

            HttpResponseMessage response = null;
            ExceptionDispatchInfo exceptionInfo = null;

            try
            {
                response = await continuation().ConfigureAwait(false);
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
                Response = response,
            };

            try
            {
                await _legacyFilter.OnActionExecutedAsync(executedContext, cancellationToken).ConfigureAwait(false);
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
