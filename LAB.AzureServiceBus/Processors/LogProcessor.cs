using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using LAB.AzureServiceBus.Messages;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace LAB.AzureServiceBus.Processors
{
    public class LogProcessor : BackgroundService
    {
        ServiceBusAdministrationClient ServiceBusAdministrationClient { get; }

        ServiceBusClient ServiceBusClient { get; }

        static string Topic => "extract";

        ConcurrentDictionary<Guid, List<Message>> Messages { get; } = new ConcurrentDictionary<Guid, List<Message>>();

        public LogProcessor(IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("ServiceBus");

            ServiceBusAdministrationClient = new ServiceBusAdministrationClient(conn);
            ServiceBusClient = new ServiceBusClient(conn);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!await ServiceBusAdministrationClient.TopicExistsAsync(Topic, stoppingToken))
            {
                await ServiceBusAdministrationClient.CreateTopicAsync(Topic, stoppingToken);
            }

            if (!await ServiceBusAdministrationClient.SubscriptionExistsAsync(Topic, nameof(LogProcessor), stoppingToken))
            {
                await ServiceBusAdministrationClient.CreateSubscriptionAsync(Topic, nameof(LogProcessor), stoppingToken);
            }

            var processor = ServiceBusClient.CreateProcessor(Topic, nameof(LogProcessor));

            processor.ProcessMessageAsync += Processor_ProcessMessageAsync;
            processor.ProcessErrorAsync += Processor_ProcessErrorAsync;

            await processor.StartProcessingAsync(stoppingToken);
        }

        async Task Processor_ProcessMessageAsync(ProcessMessageEventArgs args)
        {
            var body = JsonConvert.DeserializeObject<Message>(args.Message.Body.ToString());

            if (body == null) return;

            Console.WriteLine($"[ExtractProcessor]: Pacote recebido: {body.Value}");

            List<Message> current = Messages.TryGetValue(body.Id, out var message) ? Update(body, message) : Add(body);

            if (current.Count == (current[0].Total))
            {
                var messages = current.OrderBy(c => c.Current).Select(c => c.Value);

                Console.WriteLine($"Mensagem completa: {string.Join("", messages)}");
            }

            await args.CompleteMessageAsync(args.Message);
        }

        List<Message> Add(Message @event)
        {
            var current = new List<Message>() { @event };
            Messages.TryAdd(@event.Id, current);
            return current;
        }

        static List<Message> Update(Message @event, List<Message> message)
        {
            if (!message.Any(m => m.Current == @event.Current))
            {
                message.Add(@event);
            }

            return message;
        }

        Task Processor_ProcessErrorAsync(ProcessErrorEventArgs arg)
        {
            Console.WriteLine(arg.ErrorSource);
            return Task.CompletedTask;
        }
    }
}