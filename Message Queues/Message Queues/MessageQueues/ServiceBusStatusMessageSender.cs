using System.IO;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;

namespace Message_Queues.MessageQueues
{
    class ServiceBusStatusMessageSender
    {
        private readonly QueueClient _queueClient;

        public ServiceBusStatusMessageSender()
        {
            var connectionString =
                CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.QueueExists("statusqueue"))
                namespaceManager.CreateQueue("statusqueue");

            _queueClient = QueueClient.Create("statusqueue");
        }

        public async void SendToQueueAsync(ServiceStatus status)
        {
            var serializedStatus = JsonConvert.SerializeObject(status);
            var bytesStatus = System.Text.Encoding.UTF8.GetBytes(serializedStatus);

            using (var subMessageStream = new MemoryStream(bytesStatus))
            {
                var message = new BrokeredMessage(subMessageStream, false);
                await _queueClient.SendAsync(message);
            }
        }
    }
}
