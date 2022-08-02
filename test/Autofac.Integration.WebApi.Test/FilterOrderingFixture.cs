// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test
{
    public class FilterOrderingFixture
    {
        [Fact]
        public async void FilterOrderForAllFilterTypes()
        {
            // This test primarily serves as an example for
            // how filters and override filters interact, particularly
            // with respect to order of execution.
            var actualOrder = new List<Type>();
            Action<Type> record = t => actualOrder.Add(t);
            var builder = new ContainerBuilder();
            builder.RegisterInstance(record);

            // Filters - note Autofac controls the order of each grouping of
            // filter returned (the order of action filters, the order of exception
            // filters, etc.) but Web API is responsible for when each group
            // is executed (authorization, action, exception). Notice that it doesn't
            // matter if action filters are registered before authentication filters;
            // the appropriate group still executes at the right time.
            // From Autofac 4.0.1 onwards, services are resolved in the same order as registration,
            // which is a reversal of the approach before that version.
            builder.RegisterType<OrderTestActionFilter<A>>().AsWebApiActionFilterFor<TestControllerA>();
            builder.RegisterType<OrderTestActionFilter<B>>().AsWebApiActionFilterOverrideFor<TestControllerA>();
            builder.RegisterType<OrderTestActionFilter<C>>().AsWebApiActionFilterFor<TestControllerA>();
            builder.RegisterType<OrderTestActionFilter<D>>().AsWebApiActionFilterOverrideFor<TestControllerA>();
            builder.RegisterType<OrderTestActionFilter<E>>().AsWebApiActionFilterFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestActionFilter<F>>().AsWebApiActionFilterOverrideFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestActionFilter<G>>().AsWebApiActionFilterFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestActionFilter<H>>().AsWebApiActionFilterOverrideFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestAuthenticationFilter<A>>().AsWebApiAuthenticationFilterFor<TestControllerA>();
            builder.RegisterType<OrderTestAuthenticationFilter<B>>().AsWebApiAuthenticationFilterOverrideFor<TestControllerA>();
            builder.RegisterType<OrderTestAuthenticationFilter<C>>().AsWebApiAuthenticationFilterFor<TestControllerA>();
            builder.RegisterType<OrderTestAuthenticationFilter<D>>().AsWebApiAuthenticationFilterOverrideFor<TestControllerA>();
            builder.RegisterType<OrderTestAuthenticationFilter<E>>().AsWebApiAuthenticationFilterFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestAuthenticationFilter<F>>().AsWebApiAuthenticationFilterOverrideFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestAuthenticationFilter<G>>().AsWebApiAuthenticationFilterFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestAuthenticationFilter<H>>().AsWebApiAuthenticationFilterOverrideFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestAuthorizationFilter<A>>().AsWebApiAuthorizationFilterFor<TestControllerA>();
            builder.RegisterType<OrderTestAuthorizationFilter<B>>().AsWebApiAuthorizationFilterOverrideFor<TestControllerA>();
            builder.RegisterType<OrderTestAuthorizationFilter<C>>().AsWebApiAuthorizationFilterFor<TestControllerA>();
            builder.RegisterType<OrderTestAuthorizationFilter<D>>().AsWebApiAuthorizationFilterOverrideFor<TestControllerA>();
            builder.RegisterType<OrderTestAuthorizationFilter<E>>().AsWebApiAuthorizationFilterFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestAuthorizationFilter<F>>().AsWebApiAuthorizationFilterOverrideFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestAuthorizationFilter<G>>().AsWebApiAuthorizationFilterFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestAuthorizationFilter<H>>().AsWebApiAuthorizationFilterOverrideFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestExceptionFilter<A>>().AsWebApiExceptionFilterFor<TestControllerA>();
            builder.RegisterType<OrderTestExceptionFilter<B>>().AsWebApiExceptionFilterOverrideFor<TestControllerA>();
            builder.RegisterType<OrderTestExceptionFilter<C>>().AsWebApiExceptionFilterFor<TestControllerA>();
            builder.RegisterType<OrderTestExceptionFilter<D>>().AsWebApiExceptionFilterOverrideFor<TestControllerA>();
            builder.RegisterType<OrderTestExceptionFilter<E>>().AsWebApiExceptionFilterFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestExceptionFilter<F>>().AsWebApiExceptionFilterOverrideFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestExceptionFilter<G>>().AsWebApiExceptionFilterFor<TestControllerA>(c => c.Get());
            builder.RegisterType<OrderTestExceptionFilter<H>>().AsWebApiExceptionFilterOverrideFor<TestControllerA>(c => c.Get());

            var configuration = new HttpConfiguration();

            builder.RegisterWebApiFilterProvider(configuration);

            var container = builder.Build();

            // Set up the filter provider so we can resolve the set of filters
            var provider = new AutofacWebApiFilterProvider(container);

            // Controller action descriptor needed to request the list of filters
            var controllerDescriptor = new HttpControllerDescriptor { ControllerType = typeof(TestControllerA) };
            var methodInfo = typeof(TestControllerA).GetMethod("Get");
            var actionDescriptor = new ReflectedHttpActionDescriptor(controllerDescriptor, methodInfo);

            // Get the filters!
            // Unfortunately, this is going to get us a pretty short list
            // of things - mostly Autofac filter wrappers. Autofac does lazy
            // resolution of the actual filters internally, so you won't
            // actually see them in this list. We have to fake an execution
            // pipeline to see what's really there.
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            var filterInfos = provider.GetFilters(configuration, actionDescriptor).ToArray();

            // Fake execution of the filters to force the lazy initialization.
            // Each filter will record its type during execution.
            //
            // Set up a request and some contexts...
            var request = new HttpRequestMessage();
            request.SetConfiguration(configuration);
            var controllerContext = new HttpControllerContext { Configuration = configuration, Request = request };
            var actionContext = new HttpActionContext() { ControllerContext = controllerContext };
            var authenticationContext = new HttpAuthenticationContext(actionContext, null);
            var actionExecutedContext = new HttpActionExecutedContext(actionContext, null);
            using var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            foreach (var fi in filterInfos.Select(f => f.Instance).OfType<IAuthenticationFilter>())
            {
                await fi.AuthenticateAsync(authenticationContext, token).ConfigureAwait(false);
            }

            // Loop through each type of filter in the order Web API would
            // do it. This will give us the complete list of filters.
            foreach (var fi in filterInfos.Select(f => f.Instance).OfType<AuthorizationFilterAttribute>())
            {
                await fi.OnAuthorizationAsync(actionContext, token).ConfigureAwait(false);
            }

            // Emulate the action filter execution pipeline.
            await ExecuteContinuationFilters(actionContext, filterInfos.Select(f => f.Instance).OfType<IActionFilter>(), token).ConfigureAwait(false);

            foreach (var fi in filterInfos.Select(f => f.Instance).OfType<ExceptionFilterAttribute>())
            {
                await fi.OnExceptionAsync(actionExecutedContext, token).ConfigureAwait(false);
            }

            // Order is:
            // - Controller scoped overrides
            // - Action scoped overrides
            // - Controller scoped filters
            // - Action scoped filters
            var expectedOrder = new Type[]
            {
                typeof(OrderTestAuthenticationFilter<B>),
                typeof(OrderTestAuthenticationFilter<D>),
                typeof(OrderTestAuthenticationFilter<F>),
                typeof(OrderTestAuthenticationFilter<H>),
                typeof(OrderTestAuthenticationFilter<A>),
                typeof(OrderTestAuthenticationFilter<C>),
                typeof(OrderTestAuthenticationFilter<E>),
                typeof(OrderTestAuthenticationFilter<G>),

                typeof(OrderTestAuthorizationFilter<B>),
                typeof(OrderTestAuthorizationFilter<D>),
                typeof(OrderTestAuthorizationFilter<F>),
                typeof(OrderTestAuthorizationFilter<H>),
                typeof(OrderTestAuthorizationFilter<A>),
                typeof(OrderTestAuthorizationFilter<C>),
                typeof(OrderTestAuthorizationFilter<E>),
                typeof(OrderTestAuthorizationFilter<G>),

                typeof(OrderTestActionFilter<B>),
                typeof(OrderTestActionFilter<D>),
                typeof(OrderTestActionFilter<F>),
                typeof(OrderTestActionFilter<H>),
                typeof(OrderTestActionFilter<A>),
                typeof(OrderTestActionFilter<C>),
                typeof(OrderTestActionFilter<E>),
                typeof(OrderTestActionFilter<G>),

                typeof(OrderTestExceptionFilter<B>),
                typeof(OrderTestExceptionFilter<D>),
                typeof(OrderTestExceptionFilter<F>),
                typeof(OrderTestExceptionFilter<H>),
                typeof(OrderTestExceptionFilter<A>),
                typeof(OrderTestExceptionFilter<C>),
                typeof(OrderTestExceptionFilter<E>),
                typeof(OrderTestExceptionFilter<G>),
            };

            Assert.Equal(expectedOrder.Length, actualOrder.Count);
            for (var i = 0; i < expectedOrder.Length; i++)
            {
                Assert.Equal(expectedOrder[i], actualOrder[i]);
            }
        }

        private static async Task ExecuteContinuationFilters(HttpActionContext actionContext, IEnumerable<IActionFilter> filters, CancellationToken cancellationToken)
        {
            // Terminate the pipeline last.
            using var message = new HttpResponseMessage(HttpStatusCode.OK);
            Func<Task<HttpResponseMessage>> result = () => Task.FromResult(message);

            Func<Task<HttpResponseMessage>> ChainContinuation(Func<Task<HttpResponseMessage>> next, IActionFilter innerFilter)
            {
                return () => innerFilter.ExecuteActionFilterAsync(actionContext, cancellationToken, next);
            }

            foreach (var filterStage in filters.Reverse())
            {
                result = ChainContinuation(result, filterStage);
            }

            await result().ConfigureAwait(false);
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class A
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class B
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class C
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class D
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class E
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class F
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class G
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class H
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class OrderTestActionFilter<T> : IAutofacActionFilter
        {
            private readonly Action<Type> _record;

            public OrderTestActionFilter(Action<Type> record)
            {
                _record = record;
            }

            public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }

            public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
            {
                _record(GetType());
                return Task.FromResult(0);
            }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class OrderTestAuthenticationFilter<T> : IAutofacAuthenticationFilter
        {
            private readonly Action<Type> _record;

            public OrderTestAuthenticationFilter(Action<Type> record)
            {
                _record = record;
            }

            public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
            {
                _record(GetType());
                return Task.FromResult(0);
            }

            public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
            {
                return Task.FromResult(0);
            }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class OrderTestAuthorizationFilter<T> : IAutofacAuthorizationFilter
        {
            private readonly Action<Type> _record;

            public OrderTestAuthorizationFilter(Action<Type> record)
            {
                _record = record;
            }

            public Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
            {
                _record(GetType());
                return Task.FromResult(0);
            }
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Used in testing and via reflection.")]
        private class OrderTestExceptionFilter<T> : IAutofacExceptionFilter
        {
            private readonly Action<Type> _record;

            public OrderTestExceptionFilter(Action<Type> record)
            {
                _record = record;
            }

            public Task OnExceptionAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
            {
                _record(GetType());
                return Task.FromResult(0);
            }
        }
    }
}
