using System.Collections.Generic;
using System.Web.Http.Filters;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class AuthorizationFilterOverrideWrapperFixture
    {
        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new AuthorizationFilterOverrideWrapper(new HashSet<FilterMetadata>());
            Assert.Equal(typeof(IAuthorizationFilter), wrapper.FiltersToOverride);
        }
    }
}