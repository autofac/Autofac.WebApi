﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test
{
    public class ExceptionFilterFixture : AutofacFilterBaseFixture<TestExceptionFilter, TestExceptionFilter2, IExceptionFilter>
    {
        protected override Func<IComponentContext, TestExceptionFilter> GetFirstRegistration()
        {
            return c => new TestExceptionFilter(c.Resolve<ILogger>());
        }

        protected override Func<IComponentContext, TestExceptionFilter2> GetSecondRegistration()
        {
            return c => new TestExceptionFilter2(c.Resolve<ILogger>());
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureAsyncActionRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>(c => c.GetAsync(default));
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstAllControllersRegistration()
        {
            return r => r.AsWebApiExceptionFilterForAllControllers();
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstChainedControllersRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestControllerA>()
                         .AsWebApiExceptionFilterFor<TestControllerB>();
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstPredicateRegistration(Func<ILifetimeScope, HttpActionDescriptor, bool> predicate)
        {
            return r => r.AsWebApiExceptionFilterWhere(predicate);
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondAllControllersRegistration()
        {
            return r => r.AsWebApiExceptionFilterForAllControllers();
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondChainedControllersRegistration()
        {
            return r => r.AsWebApiExceptionFilterFor<TestControllerA>()
                .AsWebApiExceptionFilterFor<TestControllerB>();
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondPredicateRegistration(Func<HttpActionDescriptor, bool> predicate)
        {
            return r => r.AsWebApiExceptionFilterWhere(predicate);
        }

        protected override Type GetWrapperType()
        {
            return typeof(ExceptionFilterWrapper);
        }

        protected override Type GetOverrideWrapperType()
        {
            return typeof(ExceptionFilterOverrideWrapper);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideWebApiExceptionFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideWebApiExceptionFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureActionOverrideRegistration()
        {
            return builder => builder.AsWebApiExceptionFilterOverrideFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestExceptionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureControllerOverrideRegistration()
        {
            return builder => builder.AsWebApiExceptionFilterOverrideFor<TestController>();
        }
    }
}
