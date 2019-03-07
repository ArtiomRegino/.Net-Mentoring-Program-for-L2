using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Newtonsoft.Json;
using NLog;

namespace Message_Queues.MessageQueues
{
    public class ServiceBusSettingsMessageReceiver
    {
        private readonly SubscriptionClient _subscriptionClient;
        private readonly ManualResetEvent _completedResetEvent = new ManualResetEvent(false);
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ServiceBusSettingsMessageReceiver()
        {
               var connectionString =
                CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
            var namespaceManager =
                NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.TopicExists("settingtopic"))
                namespaceManager.CreateTopic("settingtopic");

            if (!namespaceManager.SubscriptionExists("settingtopic", Environment.MachineName))
                namespaceManager.CreateSubscription("settingtopic", Environment.MachineName);

            _subscriptionClient = SubscriptionClient.Create("settingtopic", Environment.MachineName);
        }

        public void CancelProcessing()
        {
            _completedResetEvent.Set();
            _subscriptionClient.CloseAsync();
        }

        public void ReceiveMessageFromSubscription(Action<ServiceSettings> callback)
        {
            Task listener = Task.Factory.StartNew(() =>
            {
                var options = new OnMessageOptions
                {
                    AutoComplete = false,
                    MaxConcurrentCalls = 1
                };

                options.ExceptionReceived += (sender, args) =>
                {
                    _logger.Error(args.Exception,
                        $"Message handler encountered an exception {args.Exception}." +
                        $"Exception message: {args.Exception.Message}." +
                        $" Inner exception: {args.Exception.InnerException}." +
                        $" Stack trace: {args.Exception.StackTrace}");
                };

                _subscriptionClient.OnMessage(message =>
                    {
                        try
                        {
                            var stream = message.GetBody<Stream>();
                            StreamReader reader = new StreamReader(stream);
                            string stringMessage = reader.ReadToEnd();
                            var settingsObject = JsonConvert.DeserializeObject(stringMessage,
                                typeof(ServiceSettings));
                            var settings = (ServiceSettings) settingsObject;

                            callback(settings);
                            message.Complete();
                        }
                        catch (Exception)
                        {
                            message.Abandon();
                        }
                        
                    }, options);
            }); 
        }
    }
}
