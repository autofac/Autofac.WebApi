using System.Collections.Generic;
using System.Web.Http.Filters;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class ExceptionFilterOverrideWrapperFixture
    {
        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new ExceptionFilterOverrideWrapper(new HashSet<FilterMetadata>());
            Assert.Equal(typeof(IExceptionFilter), wrapper.FiltersToOverride);
        }
    }
}