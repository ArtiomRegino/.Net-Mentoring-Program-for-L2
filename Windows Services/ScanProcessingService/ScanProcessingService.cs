using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.Rendering;
using Image = System.Drawing.Image;

namespace ScanProcessingService
{
    internal class ScanProcessingService
    {
        private FileSystemWatcher _watcher;
        private Thread _workThread;
        private ManualResetEvent _startWorkEvent;
        private AutoResetEvent _stopEvent;
        private Document _document;
        private Regex _imageNamePattern;
        private Regex _pdfNamePattern;
        private Regex _pdfNameReplacePattern;
        private string _fileMonitorDirectory;
        private string _fileOutputDirectory;
        private string _fileCorruptedDirectory;
        private string _pdfNameTemplate;
        private int _pdfFileNumber;
        private bool _pdfFileEnd;

        public ScanProcessingService()
        {
            SetServiceConfiguration();
            CreateDerictories(_fileMonitorDirectory, _fileOutputDirectory, _fileCorruptedDirectory);
            ValidateOutputFolder();

            _workThread = new Thread(WorkProcedure);
            _watcher = new FileSystemWatcher(_fileMonitorDirectory);
            _watcher.Created += WatcherOnCreated;
            _startWorkEvent = new ManualResetEvent(false);
            _stopEvent = new AutoResetEvent(false);
            _document = new Document();
        }

        private void CreateDerictories(params string[] directories)
        {
            foreach (var directory in directories)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
        }

        private void WorkProcedure(object obj)
        {
            do
            {
                var section = _document.AddSection();
                foreach (var filePath in Directory.EnumerateFiles(_fileMonitorDirectory))
                {
                    if (_startWorkEvent.WaitOne(TimeSpan.Zero))
                        return;

                    if (!ValidateFileName(filePath, _imageNamePattern) || !TryOpen(filePath, 3))
                        continue;

                    if (!RotateImageIfValid(filePath))
                    {
                        ExportCorruptedSequence(section, filePath);
                        continue;
                    }

                    var image = section.AddImage(filePath);
                    ConfigureImage(image);
                    section.AddPageBreak(); 
                }
                if (_pdfFileEnd)
                {
                    RenderDocument();
                    DeleteManagedSequence(section);
                    _pdfFileEnd = false;
                }
            } while (WaitHandle.WaitAny(new WaitHandle[] {_stopEvent, _startWorkEvent}, 1000) != 0);
        }

        private void ExportCorruptedSequence(Section section, string corruptedFilePath)
        {
            string fileName = Path.GetFileName(corruptedFilePath);
            File.Move(corruptedFilePath, Path.Combine(_fileCorruptedDirectory, fileName));
            var imagePaths = GetImagePathsFromFile(section);
            foreach (var filePath in imagePaths)
            {
                fileName = Path.GetFileName(filePath);
                File.Move(filePath, Path.Combine(_fileCorruptedDirectory, fileName));
            }
            _document = new Document();
        }

        private void DeleteManagedSequence(Section section)
        {
            var imagePaths = GetImagePathsFromFile(section);
            foreach (var filePath in imagePaths)
            {
                File.Delete(filePath);
            }
            _document = new Document();
        }

        public IEnumerable<string> GetImagePathsFromFile(Section section)
        {
            return section.Elements.OfType<MigraDoc.DocumentObjectModel.Shapes.Image>().Select(
                image => image.GetFilePath(_fileMonitorDirectory)).ToList();
        }

        private void ValidateOutputFolder()
        {
            Regex numberPuttern = new Regex(@"\d+");
            var existingPdfFiles = Directory.EnumerateFiles(_fileOutputDirectory);
            foreach (var filePath in existingPdfFiles)
            {
                var fileName = Path.GetFileName(filePath);
                if (ValidateFileName(filePath, _pdfNamePattern))
                {
                    var fileNumber = numberPuttern.Match(fileName);
                    int num;
                    if (int.TryParse(fileNumber.Value, out num) && _pdfFileNumber < num)
                        _pdfFileNumber = num;
                }
            }
        }

        private string GetPdfName()
        {
            _pdfFileNumber++;
            string target = _pdfFileNumber.ToString();
            return _pdfNameReplacePattern.Replace(_pdfNameTemplate, target);
        }

        /// <summary>
        /// Rotates image it has correct format.
        /// </summary>
        /// <param name="filePath">Path to image.</param>
        /// <returns>Returns false if the file does not have a valid image format.</returns>
        private bool RotateImageIfValid(string filePath)
        {
            Image image;
            try
            {
                using (image = Image.FromFile(filePath))
                {
                    if (image.Height < image.Width)
                    {
                        image.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        image.Save(filePath);
                    }
                }
            }
            catch (OutOfMemoryException ex)
            {
                return false;
            }
            return true;
        }

        private void SetServiceConfiguration()
        {
            string imageNamePattern = ConfigurationManager.AppSettings["ImageNamePattern"];
            string pdfNamePattern = ConfigurationManager.AppSettings["PdfNamePattern"];
            string pdfNameReplacePattern = ConfigurationManager.AppSettings["PdfNameReplacePattern"];
            _pdfNameTemplate = ConfigurationManager.AppSettings["PdfNameTemplate"];
            _imageNamePattern = new Regex(imageNamePattern, RegexOptions.CultureInvariant);
            _pdfNamePattern = new Regex(pdfNamePattern, RegexOptions.CultureInvariant);
            _pdfNameReplacePattern = new Regex(pdfNameReplacePattern, RegexOptions.CultureInvariant);

            string moduleFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string inputDirectory = ConfigurationManager.AppSettings["DirectoryMonitorPath"];
            string outputDirectory = ConfigurationManager.AppSettings["DirectoryOutputPath"];
            string corruptedDirectory = ConfigurationManager.AppSettings["DirectoryCorruptedSequencePath"];
            _fileMonitorDirectory = inputDirectory == "" ? Path.Combine(moduleFolder, "input"): inputDirectory;
            _fileOutputDirectory = outputDirectory == "" ? Path.Combine(moduleFolder, "output"): outputDirectory;
            _fileCorruptedDirectory = corruptedDirectory == "" ?  Path.Combine(moduleFolder, "corrupted") : corruptedDirectory;
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
            var render = new PdfDocumentRenderer
            {
                Document = _document
            };
            render.RenderDocument();
            render.Save(Path.Combine(_fileOutputDirectory, GetPdfName()));
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            _stopEvent.Set();
        }

        public void Start()
        {
            _workThread.Start();
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _watcher.EnableRaisingEvents = false;
            _startWorkEvent.Set();
            _workThread.Join();
        }

        private bool TryOpen(string path, int tryCount)
        {
            for (int i = 0; i < tryCount; i++)
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
