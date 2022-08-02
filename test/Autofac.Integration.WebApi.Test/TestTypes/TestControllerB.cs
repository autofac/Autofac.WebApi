﻿// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.WebApi.Test.TestTypes;

public class TestControllerB : TestControllerA
{
    public override IEnumerable<string> Get()
    {
        return new[] { "value1", "value2" };
    }
}
