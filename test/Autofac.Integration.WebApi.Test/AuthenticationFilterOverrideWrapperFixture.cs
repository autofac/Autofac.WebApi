using System.Collections.Generic;
using System.Web.Http.Filters;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class AuthenticationFilterOverrideWrapperFixture
    {
        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new AuthenticationFilterOverrideWrapper(new HashSet<FilterMetadata>());
            Assert.Equal(typeof(IAuthenticationFilter), wrapper.FiltersToOverride);
        }
    }
}