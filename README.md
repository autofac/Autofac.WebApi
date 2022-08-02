# Autofac.WebApi

ASP.NET Web API integration for [Autofac](https://autofac.org).

[![Build status](https://ci.appveyor.com/api/projects/status/i7fjrapyswrvy73r?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-webapi)

Please file issues and pull requests for this package in this repository rather than in the Autofac core repo.

- [Documentation](https://autofac.readthedocs.io/en/latest/integration/webapi.html)
- [NuGet](https://www.nuget.org/packages/Autofac.WebApi2/)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.WebApi)

## Quick Start

To get Autofac integrated with Web API you need to reference the Web API integration NuGet package, register your controllers, and set the dependency resolver. You can optionally enable other features as well.

```c#
protected void Application_Start()
{
  var builder = new ContainerBuilder();

  // Get your HttpConfiguration.
  var config = GlobalConfiguration.Configuration;

  // Register your Web API controllers.
  builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

  // OPTIONAL: Register the Autofac filter provider.
  builder.RegisterWebApiFilterProvider(config);

  // OPTIONAL: Register the Autofac model binder provider.
  builder.RegisterWebApiModelBinderProvider();

  // Set the dependency resolver to be Autofac.
  var container = builder.Build();
  config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
}
```

[Check out the documentation](https://autofac.readthedocs.io/en/latest/integration/webapi.html) for more usage details.

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
