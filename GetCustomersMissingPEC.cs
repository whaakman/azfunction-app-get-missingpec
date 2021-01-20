using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace AzFunctionPEC
{
    public static class GetCustomersMissingPEC
    {
        [FunctionName("GetCustomersMissingPEC")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ExecutionContext context, 
            ILogger log)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true) 
                .AddEnvironmentVariables() 
                .Build();
                
            log.LogInformation("C# HTTP trigger function processed a request.");


            string billingAccountName = config["billingAccountName"];

            var APICall = new APICall();

            var customersWithoutPec = await APICall.GetCustomersWithMissingPEC(billingAccountName);

            return new OkObjectResult(customersWithoutPec);
        }
    }
}
