using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Autofac.Builder;
using Autofac.Integration.WebApi;
using Autofac.Integration.WebApi.Test.TestTypes;

namespace Autofac.Integration.WebApi.Test
{
    public class AuthorizationFilterFixture : AutofacFilterBaseFixture<TestAuthorizationFilter, TestAuthorizationFilter2, IAuthorizationFilter>
    {
        protected override Func<IComponentContext, TestAuthorizationFilter> GetFirstRegistration()
        {
            return c => new TestAuthorizationFilter(c.Resolve<ILogger>());
        }

        protected override Func<IComponentContext, TestAuthorizationFilter2> GetSecondRegistration()
        {
            return c => new TestAuthorizationFilter2(c.Resolve<ILogger>());
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstControllerRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstActionRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstAllControllersRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterForAllControllers();
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstChainedControllersRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestControllerA>()
                         .AsWebApiAuthorizationFilterFor<TestControllerB>();
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureFirstPredicateRegistration(Func<ILifetimeScope, HttpActionDescriptor, bool> predicate)
        {
            return r => r.AsWebApiAuthorizationFilterWhere(predicate);
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondControllerRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestController>();
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondActionRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondAllControllersRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterForAllControllers();
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondChainedControllersRegistration()
        {
            return r => r.AsWebApiAuthorizationFilterFor<TestControllerA>()
                         .AsWebApiAuthorizationFilterFor<TestControllerB>();
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter2, SimpleActivatorData, SingleRegistrationStyle>> ConfigureSecondPredicateRegistration(Func<HttpActionDescriptor, bool> predicate)
        {
            return r => r.AsWebApiAuthorizationFilterWhere(predicate);
        }

        protected override Type GetWrapperType()
        {
            return typeof(AuthorizationFilterWrapper);
        }

        protected override Type GetOverrideWrapperType()
        {
            return typeof(AuthorizationFilterOverrideWrapper);
        }

        protected override Action<ContainerBuilder> ConfigureControllerFilterOverride()
        {
            return builder => builder.OverrideWebApiAuthorizationFilterFor<TestController>();
        }

        protected override Action<ContainerBuilder> ConfigureActionFilterOverride()
        {
            return builder => builder.OverrideWebApiAuthorizationFilterFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureActionOverrideRegistration()
        {
            return builder => builder.AsWebApiAuthorizationFilterOverrideFor<TestController>(c => c.Get());
        }

        protected override Action<IRegistrationBuilder<TestAuthorizationFilter, SimpleActivatorData, SingleRegistrationStyle>> ConfigureControllerOverrideRegistration()
        {
            return builder => builder.AsWebApiAuthorizationFilterOverrideFor<TestController>();
        }
    }
}