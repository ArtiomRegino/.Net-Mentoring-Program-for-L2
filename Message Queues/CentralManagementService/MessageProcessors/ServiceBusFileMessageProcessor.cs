using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CentralManagementService.FileConstruction;
using Microsoft.Azure.ServiceBus;
using NLog;

namespace CentralManagementService.MessageProcessors
{
    public class ServiceBusFileMessageProcessor : IMessageProcessor
    {
        private readonly IQueueClient _queueClient;
        private readonly Dictionary<string, ConstructedFile> _constructedFiles;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public ServiceBusFileMessageProcessor(QueueClient queueClient)
        {
            _queueClient = queueClient;
            _constructedFiles = new Dictionary<string, ConstructedFile>();
        }

        public void StartProcessing()
        {
            RegisterOnMessageHandlerAndReceiveMessages();
        }

        public void CancelProcessing()
        {
            _queueClient.CloseAsync();
        }

        private void RegisterOnMessageHandlerAndReceiveMessages()
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            _queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        private async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            try
            {
                var messageBody = message.Body;

                if (!ValidateMondatoryFields(message))
                    throw new MessageCorruptedException("Message metadata not found.");

                var sequence = GetMessageMetadata<string>("Sequence", message);
                var positionNumber = GetMessageMetadata<int>("Position", message);
                var partsNumber = GetMessageMetadata<int>("Size", message);

                var constructedFile = GetConstructedFile(sequence, partsNumber);
                var constructed = constructedFile.AppendFilePart(messageBody, positionNumber);

                if (constructed)
                {
                    SaveCostructedFile(constructedFile);
                    _constructedFiles.Remove(sequence);
                }
                    
                Console.WriteLine(positionNumber);
            }
            catch (SerializationException e)
            {
                throw new MessageCorruptedException("Message corrupted.", e);
            }

            if (token.IsCancellationRequested)
                await _queueClient.AbandonAsync(message.SystemProperties.LockToken);
            else
                // Complete the message so that it is not received again.
                // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
                // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
                // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
                // to avoid unnecessary exceptions.
                await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private void SaveCostructedFile(ConstructedFile constructedFile)
        {
            File.WriteAllBytes($"{Guid.NewGuid().ToString()}.pdf", constructedFile.ConstructedArray);
        }

        private bool ValidateMondatoryFields(Message message)
        {
            return message.UserProperties.ContainsKey("Sequence") && message.UserProperties.ContainsKey("Position") &&
                   message.UserProperties.ContainsKey("Size");
        }

        private ConstructedFile GetConstructedFile(string sequence, int partsNumber)
        {
            ConstructedFile constructedFile;

            if (_constructedFiles.ContainsKey(sequence))
            {
                constructedFile = _constructedFiles[sequence];
            }
            else
            {
                constructedFile = new ConstructedFile(partsNumber);
                _constructedFiles.Add(sequence, constructedFile);
            }

            return constructedFile;
        }

        private T GetMessageMetadata<T>(string propertyName, Message message)
        {
            object propertyObj;
            T propertyValue;

            try
            {
                propertyObj = message.UserProperties[propertyName];
            }
            catch (KeyNotFoundException e)
            {
                throw new MessageCorruptedException($"Message {propertyName} metadata not found.", e);
            }

            if (propertyObj is T value)
                propertyValue = value;
            else
                throw new MessageCorruptedException($"Message {propertyName} metadata corrupted.");

            return propertyValue;
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _logger.Error(exceptionReceivedEventArgs.Exception,
                         $"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}." +
                         $"Exception message: {exceptionReceivedEventArgs.Exception.Message}." +
                         $" Inner exception: {exceptionReceivedEventArgs.Exception.InnerException}." +
                         $" Stack trace: {exceptionReceivedEventArgs.Exception.StackTrace}");

            return Task.CompletedTask;
        }
    }
}
