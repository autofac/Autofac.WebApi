// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test;

public class ContinuationActionFilterOverrideWrapperFixture
{
    [Fact]
    public void FiltersToOverrideReturnsCorrectType()
    {
        var wrapper = new ContinuationActionFilterOverrideWrapper(new HashSet<FilterMetadata>());
        Assert.Equal(typeof(IActionFilter), wrapper.FiltersToOverride);
    }
}
