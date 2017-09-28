﻿// This software is part of the Autofac IoC container
// Copyright (c) 2012 Autofac Contributors
// http://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using System.Web.Http;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
	public class HttpConfigurationExtensionsFixture
	{
		[Fact]
		public void RegisterHttpRequestMessageAddsHandler()
		{
			var config = new HttpConfiguration();
			config.RegisterHttpRequestMessage();

			Assert.Equal(1, config.MessageHandlers.OfType<CurrentRequestHandler>().Count());
		}

		[Fact]
		public void RegisterHttpRequestMessageEnsuresHandlerAddedOnlyOnce()
		{
			var config = new HttpConfiguration();

			config.RegisterHttpRequestMessage();
			config.RegisterHttpRequestMessage();

			Assert.Equal(1, config.MessageHandlers.OfType<CurrentRequestHandler>().Count());
		}

		[Fact]
		public void RegisterHttpRequestMessageThrowsGivenNullConfig()
		{
			var exception = Assert.Throws<ArgumentNullException>(() => HttpConfigurationExtensions.RegisterHttpRequestMessage(null));

			Assert.Equal("config", exception.ParamName);
		}

	    [Fact]
	    public void IsHttpRequestMessageTrackingEnabledReturnsFalseInitially()
	    {
	        var result = HttpConfigurationExtensions.IsHttpRequestMessageTrackingEnabled;

            Assert.False(result);
	    }

		[Fact]
		public void RegisterHttpRequestMessageTurnsOnHttpRequestMessageTracking()
		{
			var config = new HttpConfiguration();

			config.RegisterHttpRequestMessage();

			Assert.True(HttpConfigurationExtensions.IsHttpRequestMessageTrackingEnabled);
		}
	}
}
