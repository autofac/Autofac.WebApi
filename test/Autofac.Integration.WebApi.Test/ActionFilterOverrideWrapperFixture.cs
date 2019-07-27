using System.Collections.Generic;
using System.Web.Http.Filters;
using Autofac.Integration.WebApi;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class ActionFilterOverrideWrapperFixture
    {
        [Fact]
        public void FiltersToOverrideReturnsCorrectType()
        {
            var wrapper = new ActionFilterOverrideWrapper(new HashSet<FilterMetadata>());
            Assert.Equal(typeof(IActionFilter), wrapper.FiltersToOverride);
        }
    }
}