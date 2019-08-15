using System.Collections.Generic;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class ContinuationActionFilterOverrideWrapperFixture
    {
        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new ContinuationActionFilterOverrideWrapper(new HashSet<FilterMetadata>());
            Assert.Equal(typeof(IActionFilter), wrapper.FiltersToOverride);
        }
    }
}