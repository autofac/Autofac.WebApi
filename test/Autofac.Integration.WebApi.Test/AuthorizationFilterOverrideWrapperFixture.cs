using System.Web.Http.Filters;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class AuthorizationFilterOverrideWrapperFixture
    {
        [Fact]
        public void MetadataKeyReturnsOverrideValue()
        {
            var wrapper = new AuthorizationFilterOverrideWrapper(new FilterMetadata());
            Assert.Equal(AutofacWebApiFilterProvider.AuthorizationFilterOverrideMetadataKey, wrapper.MetadataKey);
        }

        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new AuthorizationFilterOverrideWrapper(new FilterMetadata());
            Assert.Equal(typeof(IAuthorizationFilter), wrapper.FiltersToOverride);
        }
    }
}