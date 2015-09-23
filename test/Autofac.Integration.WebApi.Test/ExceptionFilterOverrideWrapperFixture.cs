using System.Web.Http.Filters;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class ExceptionFilterOverrideWrapperFixture
    {
        [Fact]
        public void MetadataKeyReturnsOverrideValue()
        {
            var wrapper = new ExceptionFilterOverrideWrapper(new FilterMetadata());
            Assert.Equal(AutofacWebApiFilterProvider.ExceptionFilterOverrideMetadataKey, wrapper.MetadataKey);
        }

        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new ExceptionFilterOverrideWrapper(new FilterMetadata());
            Assert.Equal(typeof(IExceptionFilter), wrapper.FiltersToOverride);
        }
    }
}