using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using LAB.AzureServiceBus.Messages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LAB.AzureServiceBus.Controllers
{
    [ApiController, Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        ServiceBusAdministrationClient ServiceBusAdministrationClient { get; }

        ServiceBusClient ServiceBusClient { get; }

        ServiceBusSender Sender { get; }

        static string Topic => "extract";

        static int QueueMessageLimit => 256;

        public MessageController(IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("ServiceBus");

            ServiceBusAdministrationClient = new ServiceBusAdministrationClient(conn);
            ServiceBusClient = new ServiceBusClient(conn);
            Sender = ServiceBusClient.CreateSender(Topic);
        }

        [HttpPost, Route("")]
        public async Task<IActionResult> SendAsync([FromBody] string message)
        {
            try
            {
                var totalBytes = System.Text.Encoding.UTF8.GetByteCount(message);
                var totalPackages = Math.Ceiling((decimal)totalBytes / QueueMessageLimit);
                int take = QueueMessageLimit;

                var id = Guid.NewGuid();
                Parallel.For(1, 2, a => { });

                for (int start = 0, i = 1; start < totalBytes; start += take, i++)
                {
                    if ((start + take) > totalBytes)
                        take = totalBytes - start;

                    var sMessage = new ServiceBusMessage(JsonConvert.SerializeObject(new Message 
                    {
                        Id = id,
                        Total = (int)totalPackages,
                        Current = i,
                        Value = message.Substring(start, take)
                    }));

                    await Sender.SendMessageAsync(sMessage);
                }

                await Sender.CloseAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }
    }
}