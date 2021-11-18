// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http.Controllers;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestContinuationActionFilterWithTransactionScope : IAutofacContinuationActionFilter
    {
        private readonly Action _before;
        private readonly Action _after;

        public TestContinuationActionFilterWithTransactionScope(Action before, Action after)
        {
            _before = before;
            _after = after;
        }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> next)
        {
            _before();

            HttpResponseMessage result;

            using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                result = await next();
            }

            _after();

            return result;
        }
    }
}
