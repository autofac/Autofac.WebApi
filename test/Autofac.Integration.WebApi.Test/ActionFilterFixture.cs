// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test
{
    public class ActionFilterFixture : AutofacFilterBaseFixture<TestActionFilter, TestActionFilter2, IActionFilter>
    {
        protected override Func<IComponentContext, TestActionFilter> GetFirstRegistration()
        {
            return c => new TestActionFilter(c.Resolve<ILogger>());
        }

        protected override Func<IComponentContext, TestActionFilter2> GetSecondRegistration()
        {
            return c => new TestActionFilter2(c.Resolve<ILogger>());
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureAsyncActionRegistration()
        {
          return r => r.AsWebApiActionFilterFor<TestController>(c => c.GetAsync(default));
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstAllControllersRegistration()
        {
            return r => r.AsWebApiActionFilterForAllControllers();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstChainedControllersRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestControllerA>()
                         .AsWebApiActionFilterFor<TestControllerB>();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstPredicateRegistration(Func<ILifetimeScope, HttpActionDescriptor, bool> predicate)
        {
            return r => r.AsWebApiActionFilterWhere(predicate);
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondAllControllersRegistration()
        {
            return r => r.AsWebApiActionFilterForAllControllers();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondChainedControllersRegistration()
        {
            return r => r.AsWebApiActionFilterFor<TestControllerA>()
                         .AsWebApiActionFilterFor<TestControllerB>();
        }

        protected override Action<IRegistrationBuilder<TestActionFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondPredicateRegistration(Func<HttpActionDescriptor, bool> predicate)
        {
            return r => r.AsWebApiActionFilterWhere(predicate);
        }

        protected override Type GetWrapperType()
        {
            return typeof(ContinuationActionFilterWrapper);
        }

        protected override Type GetOverrideWrapperType()
        {
            return typeof(ContinuationActionFilterOverrideWrapper);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideWebApiActionFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideWebApiActionFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureActionOverrideRegistration()
        {
            return r => r.AsWebApiActionFilterOverrideFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestActionFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureControllerOverrideRegistration()
        {
            return r => r.AsWebApiActionFilterOverrideFor<TestController>();
        }
    }
}
