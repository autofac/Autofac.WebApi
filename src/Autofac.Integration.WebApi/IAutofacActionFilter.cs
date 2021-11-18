// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi
{
    /// <summary>
    /// An action filter that will be created for each controller request.
    /// </summary>
    public interface IAutofacActionFilter
    {
        /// <summary>
        /// Occurs after the action method is invoked.
        /// </summary>
        /// <param name="actionExecutedContext">The context for the action.</param>
        /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
        Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken);

        /// <summary>
        /// Occurs before the action method is invoked.
        /// </summary>
        /// <param name="actionContext">The context for the action.</param>
        /// <param name="cancellationToken">A cancellation token for signaling task ending.</param>
        Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken);
    }
}
