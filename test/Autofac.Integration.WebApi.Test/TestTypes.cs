// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;

namespace Autofac.Integration.WebApi.Test
{
    public class IsAControllerNot : ApiController
    {
    }

    public class TestController : ApiController
    {
        [CustomActionFilter]
        public virtual IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }
    }

    public class TestControllerA : TestController
    {
        public override IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }
    }

    public class TestControllerB : TestControllerA
    {
        public override IEnumerable<string> Get()
        {
            return new[] { "value1", "value2" };
        }
    }

    public class InterfaceController : IHttpController
    {
        public Task<HttpResponseMessage> ExecuteAsync(HttpControllerContext controllerContext, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }

    public interface ILogger
    {
        void Log(string value);
    }

    public class Logger : ILogger, IDisposable
    {
        public void Log(string value)
        {
            Console.WriteLine(value);
        }

        public void Dispose()
        {
        }
    }

    public class CustomActionFilter : ActionFilterAttribute
    {
        public ILogger Logger { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
        }
    }


    public class Dependency
    {
    }

    public class TestModel1
    {
    }

    public class TestModel2
    {
    }

    public class TestModelBinder : IModelBinder
    {
        public Dependency Dependency { get; private set; }

        public TestModelBinder(Dependency dependency)
        {
            Dependency = dependency;
        }

        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            return true;
        }
    }

    public class TestActionFilter : IAutofacActionFilter
    {
        public ILogger Logger { get; private set; }

        public TestActionFilter(ILogger logger)
        {
            Logger = logger;
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    public class TestActionFilter2 : IAutofacActionFilter
    {
        public ILogger Logger { get; private set; }

        public TestActionFilter2(ILogger logger)
        {
            Logger = logger;
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    public class TestAuthenticationFilter : IAutofacAuthenticationFilter
    {
        public ILogger Logger { get; private set; }

        public TestAuthenticationFilter(ILogger logger)
        {
            Logger = logger;
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    public class TestAuthenticationFilter2 : IAutofacAuthenticationFilter
    {
        public ILogger Logger { get; private set; }

        public TestAuthenticationFilter2(ILogger logger)
        {
            Logger = logger;
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

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

    public class TestAuthorizationFilter2 : IAutofacAuthorizationFilter
    {
        public ILogger Logger { get; private set; }

        public TestAuthorizationFilter2(ILogger logger)
        {
            Logger = logger;
        }

        public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

    public class TestExceptionFilter : IAutofacExceptionFilter
    {
        public ILogger Logger { get; private set; }

        public TestExceptionFilter(ILogger logger)
        {
            Logger = logger;
        }

        public Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }

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

    public class TestCombinationFilter : IAutofacActionFilter, IAutofacAuthenticationFilter, IAutofacAuthorizationFilter, IAutofacExceptionFilter
    {
        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}