// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.WebApi.Test.TestTypes;

public sealed class Logger : ILogger, IDisposable
{
    public void Log(string value)
    {
        Console.WriteLine(value);
    }

    public void Dispose()
    {
    }
}
