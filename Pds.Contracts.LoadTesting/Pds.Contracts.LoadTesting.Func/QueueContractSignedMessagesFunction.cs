using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.LoadTesting.Func
{
    /// <summary>
    /// Example HTTP triggered Azure Function.
    /// </summary>
    public class QueueContractSignedMessagesFunction
    {
        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="input">The HTTP request.</param>
        /// <param name="output">ApprovedAwaitingConfirmation load test messages.</param>
        /// <returns>A task completion for async operation.</returns>
        [FunctionName("QueueContractSignedMessages")]
        public async Task QueueContractSignedMessagesAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "send")] HttpRequest input,
            [ServiceBus("%approver-load-test-topic%", Connection = "sb-connection-string")] IAsyncCollector<Message> output)
        {
            var contractTypes = new List<string>
            {
                "Main",
                "Levy",
                "AEBP",
                "DADAS",
                "EDSK"
            };

            foreach (var type in contractTypes)
            {
                for (var i = 1; i < 21; i++)
                {
                    var number = i.ToString();

                    if (number.Length == 1)
                    {
                        number = "0" + number;
                    }

                    var contractApprovedNotification = new ContractApprovedNotification
                    {
                        Ukprn = 12345678,
                        ContractVersionNumber = 1,
                        ContractNumber = $"{type}-00{number}",
                        MasterContractNumber = $"Master{type}-00{number}"
                    };

                    Message message = new Message();
                    message.ContentType = "application/json";
                    message.UserProperties.Add("Status", "ApprovedWaitingConfirmation");
                    message.Body = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(contractApprovedNotification));

                    await output.AddAsync(message);
                }
            }
        }
    }
}