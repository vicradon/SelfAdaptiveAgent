using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SelfAdaptiveAgent
{
    public class Basic
    {
        private readonly ILogger<Basic> _logger;

        public Basic(ILogger<Basic> logger)
        {
            _logger = logger;
        }

        [Function("Basic")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
