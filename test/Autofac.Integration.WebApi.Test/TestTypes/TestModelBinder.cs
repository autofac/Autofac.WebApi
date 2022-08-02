// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Autofac.Integration.WebApi.Test.TestTypes;

public class TestModelBinder : IModelBinder
{
    public Dependency Dependency { get; private set; }

    public TestModelBinder(Dependency dependency)
    {
        Dependency = dependency;
    }

    public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
    {
        return true;
    }
}
