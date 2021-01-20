using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Newtonsoft.Json.Linq;

namespace AzFunctionPEC
{
    public class APICall
    {
        private static readonly HttpClient client = new HttpClient();

         public static async Task<List<Customers>> GetCustomersWithMissingPEC(string billingAccountName)
        {
        
            string jsonBody = @"{
    ""type"": ""ActualCost"",
    ""dataSet"": {
        ""granularity"": ""monthly"",
        ""aggregation"": {
            ""totalCost"": {
                ""name"": ""Cost"",
                ""function"": ""Sum""
            },
            ""totalCostUSD"": {
                ""name"": ""CostUSD"",
                ""function"": ""Sum""
            }
        },
        ""sorting"": [
            {
                ""direction"": ""ascending"",
                ""name"": ""UsageDate""
            }
        ],
        ""grouping"": [
            {
                ""type"": ""Dimension"",
                ""name"": ""CustomerTenantId""
            },
            {
                ""type"": ""Dimension"",
                ""name"": ""CustomerName""
            }
        ],
        ""filter"": {
            ""Dimensions"": {
                ""Name"": ""PartnerEarnedCreditApplied"",
                ""Operator"": ""In"",
                ""Values"": [
                    ""false""
                ]
            }
        }
    },
    ""timeframe"": ""MonthToDate""
    }
}";
            string token = await GetToken();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("*/*"));
            client.DefaultRequestHeaders.Remove("Authorization");
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            string URI = $"https://management.azure.com/providers/Microsoft.Billing/billingAccounts/{billingAccountName}/providers/Microsoft.CostManagement/query?api-version=2019-11-01";          
           
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, URI);
            request.Content = new StringContent(jsonBody,
                                    Encoding.UTF8, 
                                    "application/json");

            HttpResponseMessage result = await client.SendAsync(request);
            string output = await result.Content.ReadAsStringAsync();
            dynamic json = JObject.Parse(output);

            List<Customers> customerList = new List<Customers>();
  
            foreach (dynamic item in json.properties.rows) {
                Customers customer = new Customers();
                customer.customerName = item[4];
                customer.subscriptionId = item[3];
                if (!String.IsNullOrEmpty(customer.subscriptionId))  
                    {
                        customerList.Add(customer);
                    }
                }

            return customerList;
        } 

         public static async Task<string> GetToken()
        {            
             // Get the access token from the managed identity
              var azureServiceTokenProvider = new AzureServiceTokenProvider();
              string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://management.azure.com");
        
              return accessToken;
        }
    }
}