using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Autofac.Integration.WebApi.Test.TestTypes
{
    public class TestOldActionFilterLocalAsync : IAutofacActionFilter
    {
        private readonly ILogger _logger;

        private static readonly AsyncLocal<string> LocalValue = new AsyncLocal<string>();

        public TestOldActionFilterLocalAsync(ILogger logger)
        {
            _logger = logger;
        }

        public Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            _logger.Log("TestOldActionFilterAsyncAccess2 OnActionExecuted Local Value: " + LocalValue.Value);

            return Task.CompletedTask;
        }

        public Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            LocalValue.Value = "localAsync";

            return Task.CompletedTask;
        }
    }
}