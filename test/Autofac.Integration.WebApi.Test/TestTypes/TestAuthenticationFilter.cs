// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestAuthenticationFilter : IAutofacAuthenticationFilter
    {
        public ILogger Logger { get; private set; }

        public TestAuthenticationFilter(ILogger logger)
        {
            Logger = logger;
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            Logger.Log(nameof(AuthenticateAsync));
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            Logger.Log(nameof(ChallengeAsync));
            return Task.FromResult(0);
        }
    }
}
