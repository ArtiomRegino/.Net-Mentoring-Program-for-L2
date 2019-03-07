using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CentralManagementService.Exstensions;
using CentralManagementService.MessageModels;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using NLog;
using Excel = Microsoft.Office.Interop.Excel;

namespace CentralManagementService.MessageProcessors
{
    public class ServiceBusStatusMessageProcessor : IMessageProcessor
    {
        private readonly IQueueClient _queueClient;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly object _locker = new object();
        private readonly string _settingsPath;

        public ServiceBusStatusMessageProcessor(QueueClient queueClient)
        {
            _queueClient = queueClient;
            _settingsPath = AppDomain.CurrentDomain.BaseDirectory + "cms\\statuses\\statuses.xlsx";
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
                var statusObject = JsonConvert.DeserializeObject(
                    System.Text.Encoding.UTF8.GetString(messageBody), typeof(ServiceStatus));

                ServiceStatus status = (ServiceStatus)statusObject;

                SaveServiceStatus(status);
                Console.WriteLine($"{status.Address}: {status.Code} - {status.Description} - {message.SystemProperties.EnqueuedTimeUtc}");
            }
            catch (SerializationException e)
            {
                throw new MessageCorruptedException("Message corrupted.", e);
            }

            if (token.IsCancellationRequested)
                await _queueClient.AbandonAsync(message.SystemProperties.LockToken);
            else
                await _queueClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        private void SaveServiceStatus(ServiceStatus status)
        {
            Excel.Workbook workbook;
            Excel._Worksheet sheet;
            var application = new Excel.Application();
            var baseDiractory = AppDomain.CurrentDomain.BaseDirectory;

            if (application == null)
                throw new InvalidOperationException("Excel is not installed on this PC.");

            application.Visible = false;

            if (!File.Exists(_settingsPath))
            {
                Directory.CreateDirectory(baseDiractory + "\\cms");
                Directory.CreateDirectory(baseDiractory + "\\cms\\statuses");

                workbook = application.Workbooks.Add("");
                sheet = workbook.ActiveSheet;

                sheet.Cells[1, 1] = "Date/time";
                sheet.Cells[1, 2] = "Address";
                sheet.Cells[1, 3] = "Code";
                sheet.Cells[1, 4] = "Description";

                workbook.SaveAs(_settingsPath, Excel.XlFileFormat.xlWorkbookDefault, Type.Missing,
                    Type.Missing, false, false, Excel.XlSaveAsAccessMode.xlShared, Type.Missing,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing);

                workbook.Close();
            }

            lock (_locker)
            {
                FileInfo fileInfo = new FileInfo(_settingsPath);
                fileInfo.TryOpenFile(FileAccess.Read, 3, 5);

                workbook = application.Workbooks.Open(_settingsPath);
                sheet = (Excel._Worksheet)workbook.ActiveSheet;

                Excel.Range last = sheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell, Type.Missing);

                var lastUsedRow = last.Row;
                sheet.Cells[lastUsedRow + 1, 1] = DateTime.Now.ToString(CultureInfo.InvariantCulture);
                sheet.Cells[lastUsedRow + 1, 2] = status.Address;
                sheet.Cells[lastUsedRow + 1, 3] = status.Code;
                sheet.Cells[lastUsedRow + 1, 4] = status.Description;

                workbook.Save();
                workbook.Close();
            }
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
