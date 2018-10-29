using System.IO;

namespace ScanProcessingService
{
    class ScanProcessingService
    {
        private FileSystemWatcher watcher;
        private string _inDir;
        private string _outDir;

        public ScanProcessingService(string inDir, string outDir)
        {
            _inDir = inDir;
            _outDir = outDir;
            watcher = new FileSystemWatcher(_inDir);
            watcher.Created += WatcherOnCreated;

            if (!Directory.Exists(inDir))
                Directory.CreateDirectory(inDir);

            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            
        }

        public void Start()
        {
            watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
        }

    }
}
