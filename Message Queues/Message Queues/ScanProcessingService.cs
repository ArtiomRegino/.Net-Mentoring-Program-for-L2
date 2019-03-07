using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using Message_Queues.MessageQueues;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using Image = System.Drawing.Image;

namespace Message_Queues
{
    internal class ScanProcessingService
    {
        private static System.Timers.Timer _completeFileTimer;
        private static System.Timers.Timer _sendStatusTimer;
        private ServiceBusStatusMessageSender _statusMessageSender;
        private IMessageSender _pdfMessageSender;
        private ServiceBusSettingsMessageReceiver _settingsMessageReceiver;
        private FileSystemWatcher _watcher;
        private Thread _workThread;
        private AutoResetEvent _startWorkEvent;
        private ServiceStatus _serviceStatus;
        private ManualResetEvent _stopEvent;
        private Document _document;
        private Regex _imageNamePattern;
        private Regex _pdfNamePattern;
        private Regex _imageNumberPattern;
        private string _fileMonitorDirectory;
        private string _fileOutputDirectory;
        private string _fileCorruptedDirectory;
        private int _pdfFileNumber;
        private int _sendStatusInterval = 10000;
        private ServiceSettings _serviceSettings;

        public ScanProcessingService()
        {
            SetServiceConfiguration();
            CreateDerictories(_fileMonitorDirectory, _fileOutputDirectory, _fileCorruptedDirectory);

            _imageNumberPattern = new Regex(@"\d+");
            _startWorkEvent = new AutoResetEvent(false);
            _stopEvent = new ManualResetEvent(false);
            _serviceStatus = new ServiceStatus
            {
                Address = Environment.MachineName,
                Code = "01",
                Description = "Waiting for processing."
            };

            _serviceSettings = new ServiceSettings
            {
                UpdateStatus = false,
                PageTimeout = 10000
            };

            ValidateOutputFolder();
            _document = new Document();
            _workThread = new Thread(WorkProcedure);
            _watcher = new FileSystemWatcher(_fileMonitorDirectory);
            _watcher.Created += WatcherOnCreated;

            
            SetupTimerSettings(ref _completeFileTimer, _serviceSettings.PageTimeout, OnCompliteFileEvent);
            SetupTimerSettings(ref _sendStatusTimer, _sendStatusInterval, OnSendStatusEvent);
            _pdfMessageSender = new ServiceBusFileMessageSender();
            _statusMessageSender = new ServiceBusStatusMessageSender();
            _settingsMessageReceiver = new ServiceBusSettingsMessageReceiver();
            _settingsMessageReceiver.ReceiveMessageFromSubscription(ApplyServiceSettings);
        }

        public void Start()
        {
            _workThread.Start();
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _stopEvent.Set();
            _workThread.Join();
            _settingsMessageReceiver.CancelProcessing();
        }

        private void CreateDerictories(params string[] directories)
        {
            foreach (var directory in directories.Where(directory => !Directory.Exists(directory)))
                Directory.CreateDirectory(directory);
        }

        private void WorkProcedure(object obj)
        {
            var addedPaths = new List<string>();
            var firstFilePath = GetSortedPaths().FirstOrDefault();
            int prevImgNumber = -1;
            var section = _document.AddSection();

            if (firstFilePath != null)
                prevImgNumber = int.Parse(GetImageNumber(firstFilePath));
            
            do
            {
                var filePaths = GetSortedPaths().ToList();

                if(filePaths.Count == 0)
                    continue;

                _serviceStatus.Code = "01";
                _serviceStatus.Description = "Processing a sequence.";

                foreach (var filePath in filePaths)
                {
                    int curImgNumber = int.Parse(GetImageNumber(filePath));
                    if (prevImgNumber + 1 != curImgNumber && prevImgNumber != curImgNumber && prevImgNumber != -1)
                    {
                        RenderAndStartNewDocument();
                        section = _document.AddSection();
                        addedPaths.Clear();
                    }

                    if (_stopEvent.WaitOne(TimeSpan.Zero))
                        return;

                    if (!TryOpen(filePath, 3) || addedPaths.Contains(filePath))
                        continue;

                    if (!RotateImageIfValid(filePath))
                    {
                        ExportCorruptedSequence(section, filePath);
                        _document = new Document();
                        section = _document.AddSection();
                        continue;
                    }

                    var image = section.AddImage(filePath);
                    
                    ConfigureImage(image);
                    addedPaths.Add(filePath);
                    ApplyServiceSettings(_serviceSettings);
                    section.AddPageBreak();
                    prevImgNumber = curImgNumber;
                }

                _serviceStatus.Code = "01";
                _serviceStatus.Description = "Waiting for processing.";

            } while (WaitHandle.WaitAny(new WaitHandle[] { _stopEvent, _startWorkEvent }, Timeout.Infinite) != 0);

            RenderDocument();
        }

        private IEnumerable<string> GetSortedPaths()
        {
            var filePaths = Directory.EnumerateFiles(_fileMonitorDirectory);
            filePaths = filePaths.Where(p => ValidateFileName(p, _imageNamePattern));
            return SortPaths(filePaths);
        }

        private IEnumerable<string> SortPaths(IEnumerable<string> paths)
        {
            var orderedPaths = paths.OrderBy(GetImageNumber);

            return orderedPaths;
        }

        private string GetImageNumber(string filePath)
        {
            if(filePath == null)
                return string.Empty;

            var matches = _imageNumberPattern.Matches(filePath);
            var lastMatch = matches[matches.Count - 1];

            return lastMatch.Value;
        }

        private void ExportCorruptedSequence(Section section, string corruptedFilePath)
        {
            MoveCorruptedFile(corruptedFilePath);
            var imagePaths = GetImagePathsFromFile(section);
            foreach (var filePath in imagePaths)
            {
                MoveCorruptedFile(filePath);
            }
        }

        private void MoveCorruptedFile(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            File.Move(filePath, Path.Combine(_fileCorruptedDirectory, fileName));
        }

        private void DeleteManagedSequence(Document document)
        {
            foreach (var filePath in document.Sections.Cast<Section>()
                .Select(GetImagePathsFromFile).SelectMany(imagePaths => imagePaths))
            {
                File.Delete(filePath);
            }
        }

        private IEnumerable<string> GetImagePathsFromFile(Section section)
        {
            return section.Elements.OfType<MigraDoc.DocumentObjectModel.Shapes.Image>().Select(
                image => image.GetFilePath(_fileMonitorDirectory)).ToList();
        }

        private void ValidateOutputFolder()
        {
            var existingPdfFiles = Directory.EnumerateFiles(_fileOutputDirectory);
            foreach (var filePath in existingPdfFiles)
            {
                var fileName = Path.GetFileName(filePath);
                if (ValidateFileName(filePath, _pdfNamePattern))
                {
                    var fileNumber = _imageNumberPattern.Match(fileName);
                    if (int.TryParse(fileNumber.Value, out int num) && _pdfFileNumber < num)
                        _pdfFileNumber = num;
                }
            }
        }

        private void RenderAndStartNewDocument()
        {
            RenderDocument();
            _document = new Document();
        }

        /// <summary>
        /// Rotates image it has correct format.
        /// </summary>
        /// <param name="filePath">Path to image.</param>
        /// <returns>Returns false if the file does not have a valid image format.</returns>
        private bool RotateImageIfValid(string filePath)
        {
            try
            {
                Image image;
                using (image = Image.FromFile(filePath))
                {
                    if (image.Height < image.Width)
                    {
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        image.Save(filePath);
                    }
                }
            }
            catch (OutOfMemoryException)
            {
                return false;
            }
            return true;
        }

        private void SetServiceConfiguration()
        {
            string imageNamePattern = ConfigurationManager.AppSettings["ImageNamePattern"];
            string pdfNamePattern = ConfigurationManager.AppSettings["PdfNamePattern"];
            _imageNamePattern = new Regex(imageNamePattern, RegexOptions.CultureInvariant);
            _pdfNamePattern = new Regex(pdfNamePattern, RegexOptions.CultureInvariant);

            string moduleFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string inputDirectory = ConfigurationManager.AppSettings["DirectoryMonitorPath"];
            string outputDirectory = ConfigurationManager.AppSettings["DirectoryOutputPath"];
            string corruptedDirectory = ConfigurationManager.AppSettings["DirectoryCorruptedSequencePath"];
            _fileMonitorDirectory = inputDirectory == "" ? Path.Combine(moduleFolder, "input") : inputDirectory;
            _fileOutputDirectory = outputDirectory == "" ? Path.Combine(moduleFolder, "output") : outputDirectory;
            _fileCorruptedDirectory = corruptedDirectory == "" ? Path.Combine(moduleFolder, "corrupted") : corruptedDirectory;
        }

        private bool ValidateFileName(string filePath, Regex filePattern)
        {
            string fileName = Path.GetFileName(filePath);
            return filePattern.IsMatch(fileName);
        }

        private void ConfigureImage(MigraDoc.DocumentObjectModel.Shapes.Image image)
        {
            image.Height = _document.DefaultPageSetup.PageHeight;
            image.RelativeVertical = RelativeVertical.Page;
            image.RelativeHorizontal = RelativeHorizontal.Page;
            image.Width = _document.DefaultPageSetup.PageWidth;
        }

        private void RenderDocument()
        {
            var renderedDocument = _document;
            var pdfRenderer = new PdfDocumentRenderer
            {
                Document = renderedDocument
            };
            _document = new Document();
            _document.AddSection();

            pdfRenderer.RenderDocument();

            using (MemoryStream ms = new MemoryStream())
            {
                pdfRenderer.Save(ms, false);
                var arr = ms.ToArray();
                File.WriteAllBytes("hello.pdf", arr);
                _pdfMessageSender.SendToQueue(ms);
            }
            DeleteManagedSequence(renderedDocument);
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            _startWorkEvent.Set();
        }

        private void SetupTimerSettings(ref System.Timers.Timer timer, double interval, ElapsedEventHandler onTimeEvent)
        {
            timer = new System.Timers.Timer {Interval = interval };
            timer.Elapsed += onTimeEvent; 
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private void ApplyServiceSettings(ServiceSettings settings)
        {
            _completeFileTimer.Interval = settings.PageTimeout * 1000;

            if (settings.UpdateStatus)
                _statusMessageSender.SendToQueueAsync(_serviceStatus);

            settings.UpdateStatus = false;
        }

        private void OnCompliteFileEvent(object source, ElapsedEventArgs e)
        {
            if (_document.LastSection != null && _document.LastSection.Elements.Count > 0)
            {
                ServiceStatus previousServiceStatus = _serviceStatus;
                _serviceStatus = new ServiceStatus
                {
                    Address = previousServiceStatus.Address,
                    Code = "01",
                    Description = "Processing a sequence."
                };

                RenderAndStartNewDocument();
                _serviceStatus = previousServiceStatus;
            }
            
            Console.WriteLine("The Elapsed event was raised at {0}", e.SignalTime);
        }

        private void OnSendStatusEvent(object source, ElapsedEventArgs e)
        {
            _statusMessageSender.SendToQueueAsync(_serviceStatus);
        }

        private bool TryOpen(string path, int tryCount)
        {
            for (var i = 0; i < tryCount; i++)
            {
                try
                {
                    var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None);
                    file.Close();

                    return true;
                }
                catch (IOException)
                {
                    Thread.Sleep(4000);
                }
            }
            return false;
        }

    }
}
