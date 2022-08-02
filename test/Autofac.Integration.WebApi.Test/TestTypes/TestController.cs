// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http;

namespace Autofac.Integration.WebApi.Test.TestTypes;

public class TestController : ApiController
{
    [CustomActionFilter]
    public virtual IEnumerable<string> Get()
    {
        return new[] { "value1", "value2" };
    }

    public virtual async Task<string> GetAsync(string arg)
    {
        await Task.Delay(1).ConfigureAwait(false);
        return arg;
    }
}
