using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class ActionFilterOverrideWrapperFixture
    {
        [Fact]
        public void MetadataKeyReturnsOverrideValue()
        {
            var wrapper = new ActionFilterOverrideWrapper(new FilterMetadata());
            Assert.Equal(AutofacWebApiFilterProvider.ActionFilterOverrideMetadataKey, wrapper.MetadataKey);
        }

        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new ActionFilterOverrideWrapper(new FilterMetadata());
            Assert.Equal(typeof(IActionFilter), wrapper.FiltersToOverride);
        }
    }
}