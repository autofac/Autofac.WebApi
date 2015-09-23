using System.Web.Http.Filters;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class AuthenticationFilterOverrideWrapperFixture
    {
        [Fact]
        public void MetadataKeyReturnsOverrideValue()
        {
            var wrapper = new AuthenticationFilterOverrideWrapper(new FilterMetadata());
            Assert.Equal(AutofacWebApiFilterProvider.AuthenticationFilterOverrideMetadataKey, wrapper.MetadataKey);
        }

        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new AuthenticationFilterOverrideWrapper(new FilterMetadata());
            Assert.Equal(typeof(IAuthenticationFilter), wrapper.FiltersToOverride);
        }
    }
}