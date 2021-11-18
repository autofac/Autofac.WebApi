// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test
{
    public class AuthenticationFilterFixture : AutofacFilterBaseFixture<TestAuthenticationFilter, TestAuthenticationFilter2, IAuthenticationFilter>
    {
        protected override Func<IComponentContext, TestAuthenticationFilter> GetFirstRegistration()
        {
            return c => new TestAuthenticationFilter(c.Resolve<ILogger>());
        }

        protected override Func<IComponentContext, TestAuthenticationFilter2> GetSecondRegistration()
        {
            return c => new TestAuthenticationFilter2(c.Resolve<ILogger>());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstAllControllersRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterForAllControllers();
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstChainedControllersRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestControllerA>()
                         .AsWebApiAuthenticationFilterFor<TestControllerB>();
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstPredicateRegistration(Func<ILifetimeScope, HttpActionDescriptor, bool> predicate)
        {
            return r => r.AsWebApiAuthenticationFilterWhere(predicate);
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureAsyncActionRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestController>(c => c.GetAsync(default));
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondAllControllersRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterForAllControllers();
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondChainedControllersRegistration()
        {
            return r => r.AsWebApiAuthenticationFilterFor<TestControllerA>()
                         .AsWebApiAuthenticationFilterFor<TestControllerB>();
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondPredicateRegistration(Func<HttpActionDescriptor, bool> predicate)
        {
            return r => r.AsWebApiAuthenticationFilterWhere(predicate);
        }

        protected override Type GetWrapperType()
        {
            return typeof(AuthenticationFilterWrapper);
        }

        protected override Type GetOverrideWrapperType()
        {
            return typeof(AuthenticationFilterOverrideWrapper);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideWebApiAuthenticationFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideWebApiAuthenticationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureActionOverrideRegistration()
        {
            return builder => builder.AsWebApiAuthenticationFilterOverrideFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthenticationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureControllerOverrideRegistration()
        {
            return builder => builder.AsWebApiAuthenticationFilterOverrideFor<TestController>();
        }
    }
}
