// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestCallbackActionFilter : IAutofacActionFilter
    {
        private readonly Action _executing;
        private readonly Action _executed;

        public TestCallbackActionFilter(Action executing, Action executed)
        {
            _executing = executing;
            _executed = executed;
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            _executing();
            return Task.FromResult(0);
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            _executed();
            return Task.FromResult(0);
        }
    }
}
