// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Filters;

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
