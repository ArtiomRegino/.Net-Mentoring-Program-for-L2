using System;
using System.IO;
using System.Threading;

namespace ScanProcessingService
{
    class ScanProcessingService
    {
        private FileSystemWatcher watcher;
        private string _inDir;
        private string _outDir;
        private Thread _workThread;

        public ScanProcessingService(string inDir, string outDir)
        {
            _inDir = inDir;
            _outDir = outDir;
            
            if (!Directory.Exists(inDir))
                Directory.CreateDirectory(inDir);

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            _workThread = new Thread(WorkProcedure);
            watcher = new FileSystemWatcher(_inDir);
            watcher.Created += WatcherOnCreated;
        }

        private void WorkProcedure(object obj)
        {
            do
            {
                foreach (var filePath in Directory.EnumerateFiles(_inDir))
                {
                    if (TryOpen(filePath, 3))
                    {
                        var outFile = Path.GetFileName(filePath);
                        File.Move(filePath, Path.Combine(_outDir, outFile));
                    }
                }
            } while (true);
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if(TryOpen(e.FullPath, 3))
                File.Move(e.FullPath, Path.Combine(_outDir, e.Name));    
        }

        public void Start()
        {
            //watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
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
