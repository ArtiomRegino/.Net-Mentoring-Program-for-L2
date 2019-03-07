using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using CentralManagementService.Exstensions;
using CentralManagementService.MessageModels;
using CsvHelper;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using NLog;

namespace CentralManagementService.MessageProcessors
{
    public class ServiceBusSettingsMessageProcessor : IMessageProcessor
    {
        private readonly ITopicClient _topicClient;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly FileSystemWatcher _fileWatcher;
        private readonly object _locker = new object();
        private readonly string _settingsPath;

        public ServiceBusSettingsMessageProcessor(TopicClient topicClient)
        {
            _topicClient = topicClient;
            _fileWatcher = new FileSystemWatcher();
            _settingsPath = AppDomain.CurrentDomain.BaseDirectory + "\\cms\\settings\\settings.csv";
        }

        public void StartProcessing()
        {
            RegisterOnSettingsHandlerAndSendMessages();
        }

        public void CancelProcessing()
        {
            _fileWatcher.EnableRaisingEvents = false;
            _topicClient.CloseAsync();
        }

        private void RegisterOnSettingsHandlerAndSendMessages()
        {
            try
            {
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Directory.CreateDirectory(baseDirectory + "\\cms");
                Directory.CreateDirectory(baseDirectory + "\\cms\\settings");

                if (!File.Exists(baseDirectory + "\\cms\\settings\\settings.csv"))
                {
                    using (File.Create(baseDirectory + "\\cms\\settings\\settings.csv")) { }
                    CreateDefaultSettings();
                }

                _fileWatcher.Path = baseDirectory + "cms\\settings";
                _fileWatcher.Changed += SettingsOnChanged;
                _fileWatcher.EnableRaisingEvents = true;
            }

            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"Message handler encountered an exception {ex}." +
                    $"Exception message: {ex.Message}." +
                    $" Inner exception: {ex.InnerException}." +
                    $" Stack trace: {ex.StackTrace}");
            }
        }

        private void SettingsOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var settings = ReadSettingsOnChanged();
            SendMessagesAsync(settings);
        }

        private void CreateDefaultSettings()
        {
            FileInfo fileInfo = new FileInfo(_settingsPath);

            fileInfo.TryOpenFile(FileAccess.Write, 3, 5);

            using (var writer = new StreamWriter(_settingsPath))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(new List<ServiceSettings>
                {
                    new ServiceSettings
                    {
                        UpdateStatus = false,
                        PageTimeout = 30
                    }
                });
            }
        }

        private ServiceSettings ReadSettingsOnChanged()
        {
            ServiceSettings serviceSettings;
            FileInfo fileInfo = new FileInfo(_settingsPath);
            fileInfo.TryOpenFile(FileAccess.Read, 3, 5);

            lock (_locker)
            {
                using (var reader = new StreamReader(_settingsPath))
                using (var csv = new CsvReader(reader))
                {
                    serviceSettings = csv.GetRecords<ServiceSettings>().FirstOrDefault();
                }
            }

            return serviceSettings;
        }

        private async Task SendMessagesAsync(ServiceSettings settings)
        {
            string messageBody = JsonConvert.SerializeObject(settings);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));

            using (var trScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _topicClient.SendAsync(message);
                trScope.Complete();
            }
        }
    }
}
