// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestContinuationActionFilter : IAutofacContinuationActionFilter
    {
        private readonly Action _before;
        private readonly Action _after;

        public TestContinuationActionFilter(Action before, Action after)
        {
            _before = before;
            _after = after;
        }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            _before();

            var result = await next().ConfigureAwait(false);

            _after();

            return result;
        }
    }
}
