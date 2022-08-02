// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test.TestTypes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public sealed class CustomActionFilterAttribute : ActionFilterAttribute
{
    public ILogger Logger { get; set; }

    public override void OnActionExecuting(HttpActionContext actionContext)
    {
    }
}
