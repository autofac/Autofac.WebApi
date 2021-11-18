// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestActionFilter3 : IAutofacActionFilter
    {
        public ILogger Logger { get; private set; }

        public TestActionFilter3(ILogger logger)
        {
            Logger = logger;
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            this.Logger.Log("TestActionFilter3.OnActionExecutingAsync");
            return Task.FromResult(0);
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            this.Logger.Log("TestActionFilter3.OnActionExecutedAsync");
            return Task.FromResult(0);
        }
    }
}
