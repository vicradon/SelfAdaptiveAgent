using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Shared;

namespace Functions
{
    public class VerifyUserFunction
    {
        private readonly Kernel _kernel;

        public VerifyUserFunction()
        {
            // Initialize the Semantic Kernel builder
            var builder = Kernel.CreateBuilder();

            // Read and validate environment variables
            var deployment = Environment.GetEnvironmentVariable("OpenAI_Deployment")
                             ?? throw new InvalidOperationException("Missing OPENAI_DEPLOYMENT");
            var endpoint = Environment.GetEnvironmentVariable("OpenAI_Endpoint")
                           ?? throw new InvalidOperationException("Missing OPENAI_ENDPOINT");
            var apiKey = Environment.GetEnvironmentVariable("OpenAI_Key")
                         ?? throw new InvalidOperationException("Missing OPENAI_KEY");

            // Configure Azure OpenAI Chat Completion
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: deployment,
                apiKey: apiKey,
                endpoint: endpoint
            );

            // Build the kernel instance
            _kernel = builder.Build();

            // Import semantic prompts as a plugin (if needed for classification)
            var pluginPath = Path.Combine(AppContext.BaseDirectory, "Skills", "KYC");
            _kernel.ImportPluginFromPromptDirectory(pluginPath, "KYC");
        }

        [Function("verify")]
        public async Task<HttpResponseData> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "verify")] HttpRequestData req)
        {
            // 1) Read incoming JSON payload
            var body = await new StreamReader(req.Body).ReadToEndAsync();

            // 2) Prepare variables for classification
            var variables = new Dictionary<string, object?>
            {
                ["userContext"] = body
            };

            // 3) Classify risk using the semantic skill
            var classificationResult = await _kernel.InvokeAsync(
                pluginName: "KYC",
                functionName: "ClassifyRisk",
                arguments: new KernelArguments(variables)
            );
            var rawRisk = classificationResult.GetValue<string>().Trim();
            var risk = Enum.Parse<RiskLevel>(rawRisk, ignoreCase: true);

            // 4) Build deterministic recommendations
            var recommendation = risk switch
            {
                RiskLevel.Low => "document verification",
                RiskLevel.Medium => "document verification and liveness check",
                RiskLevel.High => "document verification, liveness check and human review",
                _ => ""
            };

            // 5) Return the result
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(
                $"Risk={risk}; Recommended verification requirements: {recommendation}."
            );
            return response;
        }
    }
}
