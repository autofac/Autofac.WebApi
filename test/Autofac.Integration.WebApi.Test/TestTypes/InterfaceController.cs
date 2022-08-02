// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Controllers;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class InterfaceController : IHttpController
    {
        public Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
