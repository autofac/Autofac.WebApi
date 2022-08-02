// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestActionFilterWithResponse : IAutofacActionFilter
    {
        public ILogger Logger { get; private set; }

        public TestActionFilterWithResponse(ILogger logger)
        {
            Logger = logger;
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            Logger.Log("TestActionFilterWithResponse.OnActionExecutingAsync");
            actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "forbidden");
            return Task.FromResult(0);
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            Logger.Log("TestActionFilterWithResponse.OnActionExecutedAsync");
            return Task.FromResult(0);
        }
    }
}
