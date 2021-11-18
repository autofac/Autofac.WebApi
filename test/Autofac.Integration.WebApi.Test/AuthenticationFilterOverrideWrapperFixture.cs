// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
