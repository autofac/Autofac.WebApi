// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestExceptionFilter2 : IAutofacExceptionFilter
    {
        public ILogger Logger { get; private set; }

        public TestExceptionFilter2(ILogger logger)
        {
            Logger = logger;
        }

        public Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
