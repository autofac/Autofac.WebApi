// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestAuthorizationFilter : IAutofacAuthorizationFilter
    {
        public ILogger Logger { get; private set; }

        public TestAuthorizationFilter(ILogger logger)
        {
            Logger = logger;
        }

        public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
