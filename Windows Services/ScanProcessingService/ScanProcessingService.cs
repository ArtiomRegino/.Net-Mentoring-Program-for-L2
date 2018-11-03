using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        private ManualResetEvent _resetEvent;
        private AutoResetEvent _stopEvent;
        private Document document;
        private Regex _imageNamePattern;
        private Regex _pdfNamePattern;
        private string _fileMonitorDirectory;
        private string _fileOutputDirectory;
        private string _fileCorruptedDirectory;
        private string _pdfNameTemplate;
        private int _pdfFileNumber;

        public ScanProcessingService()
        {
            CreateDerictories(_fileMonitorDirectory, _fileOutputDirectory, _fileCorruptedDirectory);
            SetServiceConfiguration();
            ValidateOutputFolder();

            _workThread = new Thread(WorkProcedure);
            _watcher = new FileSystemWatcher(_fileMonitorDirectory);
            _watcher.Created += WatcherOnCreated;
            _resetEvent = new ManualResetEvent(false);
            _stopEvent = new AutoResetEvent(false);
            document = new Document();
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
            var section = document.AddSection();
            
            do
            {
                foreach (var filePath in Directory.EnumerateFiles(_fileMonitorDirectory))
                {
                    if (_resetEvent.WaitOne(TimeSpan.Zero))
                        return;

                    if (!ValidateImageName(filePath) || !TryOpen(filePath, 3))
                        continue;

                    Image img = GetImageIfValid(filePath);

                    if (img == null)
                    {
                        ExportCorruptedSequence();
                        continue;
                    }

                    var image = section.AddImage(filePath);

                    if (img.Height < img.Width)
                    {
                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        img.Save(filePath);
                    }

                    ConfigureImage(image);
                    section.AddPageBreak(); 
                        
                    //var outFile = Path.GetFileName(filePath);
                    //File.Move(filePath, Path.Combine(_outDir, outFile));
                }
                RenderDocument();

            } while (WaitHandle.WaitAny(new WaitHandle[] {_stopEvent, _resetEvent}, 1000) != 0);
        }

        private void ExportCorruptedSequence()
        {
            throw new NotImplementedException();
        }

        //TODO: !
        private void ValidateOutputFolder()
        {
            var existingPdfFiles = Directory.EnumerateFiles(_fileOutputDirectory);
            foreach (var filePath in existingPdfFiles)
            {
                Path.GetFileName(filePath);
            }
        }

        public string GetPdfName()
        {
            _pdfFileNumber++;
            string target = _pdfFileNumber.ToString();
            return _pdfNamePattern.Replace(_pdfNameTemplate, target);
        }

        /// <summary>
        /// Gets valid image.
        /// </summary>
        /// <param name="filename">Path to image.</param>
        /// <returns>Returns null if the file does not have a valid image format.</returns>
        Image GetImageIfValid(string filename)
        {
            Image newImage;
            try
            {
                newImage = Image.FromFile(filename);
            }
            catch (OutOfMemoryException ex)
            {
                return null;
            }
            return newImage;
        }

        public void SetServiceConfiguration()
        {
            string imageNamePattern = ConfigurationManager.AppSettings["ImageNamePattern"];
            string pdfNamePattern = ConfigurationManager.AppSettings["PdfNamePattern"];
            _pdfNameTemplate = ConfigurationManager.AppSettings["PdfNameTemplate"];
            _imageNamePattern = new Regex(imageNamePattern, RegexOptions.CultureInvariant);
            _pdfNamePattern = new Regex(pdfNamePattern, RegexOptions.CultureInvariant);

            string moduleFolder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string inputDirectory = ConfigurationManager.AppSettings["DirectoryMonitorPath"];
            string outputDirectory = ConfigurationManager.AppSettings["DirectoryOutputPath"];
            string corruptedDirectory = ConfigurationManager.AppSettings["DirectoryCorruptedSequencePath"];
            _fileMonitorDirectory = inputDirectory == "" ? inputDirectory : Path.Combine(moduleFolder, "input");
            _fileOutputDirectory = outputDirectory == "" ? outputDirectory : Path.Combine(moduleFolder, "output");
            _fileCorruptedDirectory = corruptedDirectory == "" ? corruptedDirectory : Path.Combine(moduleFolder, "output");
        }

        public bool ValidateImageName(string imagePath)
        {
            string imageName = Path.GetFileName(imagePath);
            return _imageNamePattern.IsMatch(imageName);
        }

        private void ConfigureImage(MigraDoc.DocumentObjectModel.Shapes.Image image)
        {
            image.Height = document.DefaultPageSetup.PageHeight;
            image.RelativeVertical = RelativeVertical.Page;
            image.RelativeHorizontal = RelativeHorizontal.Page;
            image.Width = document.DefaultPageSetup.PageWidth;
        }

        private void RenderDocument()
        {
            var render = new PdfDocumentRenderer
            {
                Document = document
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
            //watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _resetEvent.Set();
            _workThread.Join();
            //watcher.EnableRaisingEvents = false;
        }

        public bool TryOpen(string path, int tryCount)
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
