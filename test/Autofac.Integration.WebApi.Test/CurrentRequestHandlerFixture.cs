using System.Net.Http;
using System.Web.Http.Hosting;
using Autofac.Core.Lifetime;
using Xunit;

namespace Autofac.Integration.WebApi.Test
{
    public class CurrentRequestHandlerFixture
    {
        [Fact]
        public void HandlerUpdatesDependencyScopeWithHttpRequestMessage()
        {
            var request = new HttpRequestMessage();
            var lifetimeScope = new ContainerBuilder().Build().BeginLifetimeScope(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
            var scope = new AutofacWebApiDependencyScope(lifetimeScope);
            request.Properties.Add(HttpPropertyKeys.DependencyScope, scope);

            CurrentRequestHandler.UpdateScopeWithHttpRequestMessage(request);

            Assert.Equal(request, scope.GetService(typeof(HttpRequestMessage)));
        }
    }
}
