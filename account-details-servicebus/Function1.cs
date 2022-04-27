using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;

namespace account_details_servicebus
{
    public static class Function1
    {
        private static AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
        static void Program()
        {
            _circuitBreakerPolicy = Policy
                    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .CircuitBreakerAsync(1, TimeSpan.FromSeconds(10), OnBreak, OnReset, OnHalfOpen);
        }

        [FunctionName("Function1")]
        public static async Task Run([ServiceBusTrigger("cdnsqueue", Connection = "ServiceBusConnectionString")]CustomerDetails myQueueItem, ILogger log)
        {

            var client = new HttpClient();

            var custDetails = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("FirstName", myQueueItem.FirstName),
                new KeyValuePair<string, string>("LastName", myQueueItem.LastName),
                new KeyValuePair<string, string>("PhoneNumber", myQueueItem.PhoneNumber),
                new KeyValuePair<string, string>("EmailId", myQueueItem.EmailId),
                new KeyValuePair<string, string>("UserType", myQueueItem.UserType),
                new KeyValuePair<string, string>("UniversityName", myQueueItem.UniversityName),
            };

            var content = new FormUrlEncodedContent(custDetails);


            var apiClient = new HttpClient();
            int i = 0;
            while (true)
            {
                i++;
                Console.WriteLine($"{i}. Start calling to Web API");
                Console.WriteLine("\n");
                Console.WriteLine("-------------------------------------------------------------------------------------------");
                // Start calling to WebAPI
                var apiResponse = new HttpResponseMessage();
                try
                {
                    apiResponse = await _circuitBreakerPolicy.ExecuteAsync(
                        () => apiClient.PostAsync("https://localhost:44354/api/CustomerAccount/PostCustomerDetails", content)
                        );
                    var json = await apiResponse.Content.ReadAsStringAsync();
                    // End calling to WebAPI

                    Console.WriteLine($"Http Status Code: {apiResponse.StatusCode}");
                    Console.WriteLine("\n");
                    Console.WriteLine($"Response: {json}");
                    Console.WriteLine("\n");
                    Console.WriteLine($"{i}. End calling to Web API");
                    Console.WriteLine("\n");
                    Console.WriteLine("-------------------------------------------------------------------------------------------");
                    Console.WriteLine("Type any key and press Enter to make new calling to Web API");
                    Console.WriteLine("-------------------------------------------------------------------------------------------");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            }
        }

        private static void OnHalfOpen()
        {
            Console.WriteLine("Connection half open - Circuit Breaker State is HALF-OPEN");
        }

        private static void OnReset(Context context)
        {
            Console.WriteLine("Connection reset - Circuit Breaker State is CLOSED");
        }

        private static void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, Context context)
        {
            Console.WriteLine("Connection is Closed - Circuit Breaker State is OPEN");
        }
    }
}
