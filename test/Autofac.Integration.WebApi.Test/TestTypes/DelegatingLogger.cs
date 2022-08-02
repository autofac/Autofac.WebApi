// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class DelegatingLogger : ILogger
    {
        private readonly Action<string> _onLog;

        public DelegatingLogger(Action<string> onLog)
        {
            _onLog = onLog;
        }

        public void Log(string value)
        {
            _onLog(value);
        }
    }
}
